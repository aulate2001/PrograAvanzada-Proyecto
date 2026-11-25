using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pedidos360.Models;
using Pedidos360.Services.Interfaces;
using Pedidos360.ViewModels.Ordenes;

namespace Pedidos360.Controllers.Api;

// RF-06: la API de ordenes exige usuario autenticado con rol Admin o Vendedor.
// Si no hay sesion responde 401 y si el rol no alcanza responde 403 (configurado en Program.cs).
[ApiController]
[Authorize(Roles = "Admin,Vendedor")]
[Route("api/ordenes")]
public class OrdenesApiController(IOrdenServicioService ordenes, ILogger<OrdenesApiController> logger) : ControllerBase
{
    // GET /api/ordenes            -> todas las ordenes
    // GET /api/ordenes?estado=2   -> solo las de ese estado (filtro AJAX de RF-03)
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] EstadoOrden? estado)
    {
        if (estado.HasValue && !Enum.IsDefined(estado.Value))
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "El estado no es válido." });
        }

        try
        {
            return Ok(await ordenes.ObtenerAsync(estado));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al listar las órdenes de servicio.");
            return Problem(title: "No se pudieron consultar las órdenes.", statusCode: 500);
        }
    }

    // GET /api/ordenes/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var orden = await ordenes.ObtenerPorIdAsync(id);
        return orden is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "La orden no existe." })
            : Ok(orden);
    }

    // PUT /api/ordenes/{id}/estado   cuerpo: { "estado": 2 }
    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, ActualizarEstadoEntrada entrada)
    {
        try
        {
            return Ok(await ordenes.ActualizarEstadoAsync(id, entrada.Estado!.Value));
        }
        catch (ReglaNegocioException error)
        {
            return new ObjectResult(new ProblemDetails
            {
                Status = error.Estado,
                Title = error.Message,
                Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
            }) { StatusCode = error.Estado };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al actualizar el estado de la orden {OrdenId}.", id);
            return Problem(title: "No se pudo actualizar el estado de la orden.", statusCode: 500);
        }
    }
}
