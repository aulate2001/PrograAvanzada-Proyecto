using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Models;
using Pedidos360.Services.Interfaces;
using Pedidos360.ViewModels.Ordenes;

namespace Pedidos360.Services
{
    public class OrdenServicioService : IOrdenServicioService
    {
        private readonly ApplicationDbContext _context;

        public OrdenServicioService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lista las ordenes; si viene un estado, filtra por ese estado.
        public async Task<List<OrdenServicioDto>> ObtenerAsync(EstadoOrden? estado)
        {
            var consulta = _context.OrdenesServicio
                .AsNoTracking()
                .Include(orden => orden.Cliente)
                .Include(orden => orden.TipoServicio)
                .AsQueryable();

            if (estado.HasValue)
            {
                consulta = consulta.Where(orden => orden.Estado == estado.Value);
            }

            var ordenes = await consulta
                .OrderByDescending(orden => orden.FechaIngreso)
                .ToListAsync();

            return ordenes.Select(ConvertirADto).ToList();
        }

        public async Task<OrdenServicioDto?> ObtenerPorIdAsync(int id)
        {
            var orden = await _context.OrdenesServicio
                .AsNoTracking()
                .Include(actual => actual.Cliente)
                .Include(actual => actual.TipoServicio)
                .FirstOrDefaultAsync(actual => actual.Id == id);

            return orden is null ? null : ConvertirADto(orden);
        }

        // El estado solo puede avanzar al siguiente. Al pasar a Entregado se asigna la fecha de entrega.
        public async Task<OrdenServicioDto> ActualizarEstadoAsync(int id, EstadoOrden nuevoEstado)
        {
            var orden = await _context.OrdenesServicio
                .Include(actual => actual.Cliente)
                .Include(actual => actual.TipoServicio)
                .FirstOrDefaultAsync(actual => actual.Id == id)
                ?? throw new ReglaNegocioException(StatusCodes.Status404NotFound, "La orden no existe.");

            var siguiente = ObtenerSiguienteEstado(orden.Estado);
            if (siguiente is null)
            {
                throw new ReglaNegocioException(StatusCodes.Status409Conflict, "La orden ya fue entregada y no puede cambiar de estado.");
            }

            if (nuevoEstado != siguiente)
            {
                throw new ReglaNegocioException(StatusCodes.Status422UnprocessableEntity,
                    $"La orden está en {NombreEstado(orden.Estado)} y solo puede pasar a {NombreEstado(siguiente.Value)}.");
            }

            orden.Estado = nuevoEstado;
            if (nuevoEstado == EstadoOrden.Entregado)
            {
                orden.FechaEntrega = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return ConvertirADto(orden);
        }

        private static EstadoOrden? ObtenerSiguienteEstado(EstadoOrden actual)
        {
            return actual switch
            {
                EstadoOrden.Pendiente => EstadoOrden.EnReparacion,
                EstadoOrden.EnReparacion => EstadoOrden.Listo,
                EstadoOrden.Listo => EstadoOrden.Entregado,
                _ => null
            };
        }

        // Estado en prosa para mostrar al usuario.
        public static string NombreEstado(EstadoOrden estado)
        {
            return estado switch
            {
                EstadoOrden.Pendiente => "Pendiente",
                EstadoOrden.EnReparacion => "En reparación",
                EstadoOrden.Listo => "Listo",
                EstadoOrden.Entregado => "Entregado",
                _ => "Desconocido"
            };
        }

        private static OrdenServicioDto ConvertirADto(OrdenServicio orden)
        {
            var siguiente = ObtenerSiguienteEstado(orden.Estado);
            return new OrdenServicioDto
            {
                Id = orden.Id,
                Equipo = orden.Equipo,
                Cliente = orden.Cliente?.Nombre ?? string.Empty,
                TipoServicio = orden.TipoServicio?.Nombre ?? string.Empty,
                CostoEstimado = orden.CostoEstimado,
                Estado = (int)orden.Estado,
                EstadoTexto = NombreEstado(orden.Estado),
                FechaIngreso = orden.FechaIngreso,
                FechaEntrega = orden.FechaEntrega,
                SiguienteEstado = siguiente is null ? null : (int)siguiente.Value,
                SiguienteEstadoTexto = siguiente is null ? null : NombreEstado(siguiente.Value)
            };
        }
    }
}
