using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Models;
using Pedidos360.Services.Interfaces;
using Pedidos360.ViewModels;

namespace Pedidos360.Services;

public class PedidoService : IPedidoService
{
    private readonly ApplicationDbContext _db;

    public PedidoService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PedidoCreateViewModel> ObtenerFormularioAsync()
    {
        return new PedidoCreateViewModel
        {
            Clientes = await ObtenerClientesAsync(),
            Productos = await ObtenerProductosAsync(),
            Lineas = [new PedidoLineaViewModel()]
        };
    }

    public async Task<List<SelectListItem>> ObtenerClientesAsync()
    {
        return await _db.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
            .ToListAsync();
    }

    public async Task<List<PedidoProductoViewModel>> ObtenerProductosAsync()
    {
        return await _db.Productos
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new PedidoProductoViewModel
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Precio = p.Precio,
                ImpuestoPorc = p.ImpuestoPorc,
                Stock = p.Stock
            })
            .ToListAsync();
    }

    public async Task<(bool Exito, string? Mensaje)> CrearAsync(int clienteId, List<PedidoLineaViewModel> lineas, string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return (false, "Debe iniciar sesión para confirmar un pedido.");
        }

        if (clienteId <= 0)
        {
            return (false, "Debe seleccionar un cliente.");
        }

        if (lineas is null)
        {
            return (false, "Debe agregar al menos un producto al pedido.");
        }

        var lineasValidas = lineas.Where(l => l is not null).ToList();

        if (lineasValidas.Count == 0 || lineasValidas.All(l => l.ProductoId <= 0))
        {
            return (false, "Debe agregar al menos un producto al pedido.");
        }

        var productosUnicos = new HashSet<int>();
        foreach (var linea in lineasValidas)
        {
            if (linea.ProductoId <= 0)
            {
                continue;
            }

            if (!productosUnicos.Add(linea.ProductoId))
            {
                return (false, "No puedes repetir el mismo producto en el mismo pedido.");
            }
        }

        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var clienteExiste = await _db.Clientes
                .AsNoTracking()
                .AnyAsync(c => c.Id == clienteId);
            if (!clienteExiste)
            {
                return (false, "El cliente seleccionado no existe.");
            }

            var productoIds = lineasValidas.Select(l => l.ProductoId).ToArray();
            var productos = await _db.Productos
                .AsNoTracking()
                .Where(p => productoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var pedido = new Pedido
            {
                ClienteId = clienteId,
                UsuarioId = usuarioId,
                Fecha = DateTime.UtcNow,
                Estado = EstadoPedido.Confirmado,
                Detalles = new List<PedidoDetalle>()
            };

            decimal subtotalTotal = 0m;
            decimal impuestosTotal = 0m;

            foreach (var linea in lineasValidas)
            {
                if (linea.ProductoId <= 0)
                {
                    return (false, "Cada línea debe tener un producto válido.");
                }

                if (linea.Cantidad <= 0)
                {
                    return (false, "La cantidad debe ser mayor a 0.");
                }

                if (!productos.TryGetValue(linea.ProductoId, out var producto))
                {
                    return (false, "Uno de los productos seleccionados ya no existe.");
                }

                if (!producto.Activo)
                {
                    return (false, $"El producto {producto.Nombre} ya no está disponible.");
                }

                if (producto.Stock < linea.Cantidad)
                {
                    return (false, $"No hay stock suficiente para {producto.Nombre}.");
                }

                if (!Enum.IsDefined(linea.TipoDescuento))
                {
                    return (false, $"El tipo de descuento de {producto.Nombre} no es válido.");
                }

                var baseLinea = producto.Precio * linea.Cantidad;
                var valorDescuento = decimal.Round(linea.Descuento, 2, MidpointRounding.AwayFromZero);
                decimal descuentoAplicado;

                switch (linea.TipoDescuento)
                {
                    case TipoDescuento.Porcentaje:
                        if (valorDescuento < 0m || valorDescuento > 100m)
                        {
                            return (false, $"El descuento de {producto.Nombre} debe estar entre 0 y 100 %.");
                        }
                        descuentoAplicado = decimal.Round(baseLinea * (valorDescuento / 100m), 2, MidpointRounding.AwayFromZero);
                        break;

                    case TipoDescuento.Monto:
                        if (valorDescuento < 0m || valorDescuento > baseLinea)
                        {
                            return (false, $"El descuento de {producto.Nombre} excede el valor de la línea.");
                        }
                        descuentoAplicado = valorDescuento;
                        break;

                    default:
                        return (false, $"El tipo de descuento de {producto.Nombre} no es válido.");
                }

                var subtotalLinea = decimal.Round(baseLinea - descuentoAplicado, 2, MidpointRounding.AwayFromZero);
                var impuestoLinea = decimal.Round(subtotalLinea * (producto.ImpuestoPorc / 100m), 2, MidpointRounding.AwayFromZero);

                pedido.Detalles.Add(new PedidoDetalle
                {
                    ProductoId = producto.Id,
                    Cantidad = linea.Cantidad,
                    PrecioUnit = producto.Precio,
                    TipoDescuento = linea.TipoDescuento,
                    Descuento = valorDescuento,
                    ImpuestoPorc = producto.ImpuestoPorc,
                    TotalLinea = subtotalLinea + impuestoLinea
                });

                subtotalTotal += subtotalLinea;
                impuestosTotal += impuestoLinea;
            }

            foreach (var linea in lineasValidas.OrderBy(l => l.ProductoId))
            {
                var stockActualizado = await _db.Productos
                    .Where(p => p.Id == linea.ProductoId && p.Activo && p.Stock >= linea.Cantidad)
                    .ExecuteUpdateAsync(actual => actual
                        .SetProperty(p => p.Stock, p => p.Stock - linea.Cantidad));

                if (stockActualizado == 0)
                {
                    var nombre = productos[linea.ProductoId].Nombre;
                    return (false, $"El stock de {nombre} cambió o ya no es suficiente. Vuelva a intentarlo.");
                }
            }

            pedido.Subtotal = subtotalTotal;
            pedido.Impuestos = impuestosTotal;
            pedido.Total = subtotalTotal + impuestosTotal;
            _db.Pedidos.Add(pedido);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, null);
        }
        catch (DbUpdateException)
        {
            return (false, "No se pudo guardar el pedido. Verifique los datos e intente nuevamente.");
        }
    }
}
