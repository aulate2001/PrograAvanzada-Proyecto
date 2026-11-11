using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Data;
using Pedidos360.Services.Interfaces;
using Pedidos360.ViewModels;

namespace Pedidos360.Controllers;

[Authorize(Roles = "Admin,Vendedor")]
public class PedidosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPedidoService _pedidoService;

    public PedidosController(ApplicationDbContext db, IPedidoService pedidoService)
    {
        _db = db;
        _pedidoService = pedidoService;
    }

    public async Task<IActionResult> Index()
    {
        var pedidos = await _db.Pedidos.AsNoTracking()
            .Include(p => p.Cliente)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();
        return View(pedidos);
    }

    public async Task<IActionResult> Details(int id)
    {
        var pedido = await _db.Pedidos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pedido is null) return NotFound();
        return View(pedido);
    }

    public async Task<IActionResult> Create()
    {
        var vm = await _pedidoService.ObtenerFormularioAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PedidoCreateViewModel vm)
    {
        vm.Lineas ??= [];
        vm.Lineas.RemoveAll(linea => linea is null);
        if (vm.Lineas.Count == 0)
        {
            vm.Lineas.Add(new PedidoLineaViewModel());
        }

        if (!ModelState.IsValid)
        {
            var formulario = await _pedidoService.ObtenerFormularioAsync();
            vm.Clientes = formulario.Clientes;
            vm.Productos = formulario.Productos;
            return View(vm);
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var resultado = await _pedidoService.CrearAsync(vm.ClienteId, vm.Lineas, usuarioId ?? string.Empty);

        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje ?? "No se pudo confirmar el pedido.");
            var formulario = await _pedidoService.ObtenerFormularioAsync();
            vm.Clientes = formulario.Clientes;
            vm.Productos = formulario.Productos;
            return View(vm);
        }

        TempData["Exito"] = "Pedido confirmado correctamente.";
        return RedirectToAction(nameof(Create));
    }
}
