using Pedidos360.Models;
using Pedidos360.ViewModels;

namespace Pedidos360.Services.Interfaces
{
    public interface IClienteService
    {
        Task<ClienteIndexViewModel> ObtenerPaginaAsync(string? busqueda, int pagina);
        Task<Cliente?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteCedulaAsync(string cedula, int? idExcluir = null);
        Task<bool> CrearAsync(Cliente cliente);
        Task<bool> ActualizarAsync(Cliente cliente);
        Task<bool> TieneOrdenesAsociadasAsync(int id);
        Task<bool> EliminarSiNoTieneOrdenesAsync(int id);
    }
}
