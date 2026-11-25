using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Models;
using Pedidos360.Services.Interfaces;
using Pedidos360.ViewModels;

namespace Pedidos360.Services
{
    public class ClienteService : IClienteService
    {
        private const int ClientesPorPagina = 10;
        private readonly ApplicationDbContext _context;

        public ClienteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClienteIndexViewModel> ObtenerPaginaAsync(string? busqueda, int pagina)
        {
            var termino = busqueda?.Trim();
            var consulta = _context.Set<Cliente>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                consulta = consulta.Where(cliente =>
                    cliente.Nombre.Contains(termino) || cliente.Cedula.Contains(termino));
            }

            var total = await consulta.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)ClientesPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);

            return new ClienteIndexViewModel
            {
                Clientes = await consulta
                    .OrderBy(cliente => cliente.Nombre)
                    .ThenBy(cliente => cliente.Id)
                    .Skip((pagina - 1) * ClientesPorPagina)
                    .Take(ClientesPorPagina)
                    .ToListAsync(),
                Busqueda = termino,
                Pagina = pagina,
                TotalPaginas = totalPaginas,
                TotalClientes = total
            };
        }

        public Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            return _context.Set<Cliente>().AsNoTracking().FirstOrDefaultAsync(cliente => cliente.Id == id);
        }

        public Task<bool> ExisteCedulaAsync(string cedula, int? idExcluir = null)
        {
            var consulta = _context.Set<Cliente>().AsNoTracking().Where(cliente => cliente.Cedula == cedula.Trim());

            if (idExcluir.HasValue)
            {
                consulta = consulta.Where(cliente => cliente.Id != idExcluir.Value);
            }

            return consulta.AnyAsync();
        }

        public async Task<bool> CrearAsync(Cliente cliente)
        {
            cliente.Cedula = cliente.Cedula.Trim();
            if (await ExisteCedulaAsync(cliente.Cedula))
            {
                return false;
            }

            _context.Set<Cliente>().Add(cliente);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException exception) when (EsCedulaDuplicada(exception))
            {
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(Cliente cliente)
        {
            var existente = await _context.Set<Cliente>().FirstOrDefaultAsync(actual => actual.Id == cliente.Id);
            if (existente is null || await ExisteCedulaAsync(cliente.Cedula, cliente.Id))
            {
                return false;
            }

            existente.TipoIdentificacion = cliente.TipoIdentificacion;
            existente.Cedula = cliente.Cedula.Trim();
            existente.Nombre = cliente.Nombre.Trim();
            existente.Correo = cliente.Correo.Trim();
            existente.Telefono = cliente.Telefono.Trim();
            existente.Direccion = cliente.Direccion.Trim();

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException exception) when (EsCedulaDuplicada(exception))
            {
                return false;
            }
        }

        // RF-01: un cliente no se puede eliminar si tiene ordenes asociadas.
        // Hoy las ordenes del sistema son los pedidos. Cuando se agreguen las ordenes de servicio (RF-02),
        // basta con sumar aqui la consulta a esa tabla para que la regla las cubra tambien.
        public Task<bool> TieneOrdenesAsociadasAsync(int id)
        {
            return _context.Set<Pedido>().AsNoTracking().AnyAsync(pedido => pedido.ClienteId == id);
        }

        public async Task<bool> EliminarSiNoTieneOrdenesAsync(int id)
        {
            var cliente = await _context.Set<Cliente>().FirstOrDefaultAsync(actual => actual.Id == id);
            if (cliente is null || await TieneOrdenesAsociadasAsync(id))
            {
                return false;
            }

            _context.Set<Cliente>().Remove(cliente);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException exception) when (EsRestriccionDeRelacion(exception))
            {
                return false;
            }
        }

        private static bool EsCedulaDuplicada(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException &&
                   (sqlException.Number == 2601 || sqlException.Number == 2627);
        }

        private static bool EsRestriccionDeRelacion(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException && sqlException.Number == 547;
        }
    }
}
