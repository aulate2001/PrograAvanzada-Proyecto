using Pedidos360.Models;
using Pedidos360.ViewModels.Ordenes;

namespace Pedidos360.Services.Interfaces
{
    public interface IOrdenServicioService
    {
        Task<List<OrdenServicioDto>> ObtenerAsync(EstadoOrden? estado);
        Task<OrdenServicioDto?> ObtenerPorIdAsync(int id);
        Task<OrdenServicioDto> ActualizarEstadoAsync(int id, EstadoOrden nuevoEstado);
    }
}
