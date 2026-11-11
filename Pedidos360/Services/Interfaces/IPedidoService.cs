using Microsoft.AspNetCore.Mvc.Rendering;
using Pedidos360.ViewModels;

namespace Pedidos360.Services.Interfaces;

public interface IPedidoService
{
    Task<PedidoCreateViewModel> ObtenerFormularioAsync();
    Task<(bool Exito, string? Mensaje)> CrearAsync(int clienteId, List<PedidoLineaViewModel> lineas, string usuarioId);
    Task<List<PedidoProductoViewModel>> ObtenerProductosAsync();
    Task<List<SelectListItem>> ObtenerClientesAsync();
}
