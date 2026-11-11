using Microsoft.AspNetCore.Mvc.Rendering;
using Pedidos360.Models;
using System.ComponentModel.DataAnnotations;

namespace Pedidos360.ViewModels;

public class PedidoLineaViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto.")]
    [Display(Name = "Producto")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; } = 1;

    [Display(Name = "Tipo de descuento")]
    public TipoDescuento TipoDescuento { get; set; } = TipoDescuento.Porcentaje;

    [Range(typeof(decimal), "0", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El descuento no puede ser negativo.")]
    [Display(Name = "Descuento")]
    public decimal Descuento { get; set; }
}

public class PedidoCreateViewModel
{
    [Display(Name = "Cliente")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente.")]
    public int ClienteId { get; set; }

    public List<SelectListItem> Clientes { get; set; } = [];
    public List<PedidoProductoViewModel> Productos { get; set; } = [];
    public List<PedidoLineaViewModel> Lineas { get; set; } = [new PedidoLineaViewModel()];
}

public class PedidoProductoViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal ImpuestoPorc { get; set; }
    public int Stock { get; set; }
}
