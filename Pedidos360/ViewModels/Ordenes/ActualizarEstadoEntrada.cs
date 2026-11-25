using System.ComponentModel.DataAnnotations;
using Pedidos360.Models;

namespace Pedidos360.ViewModels.Ordenes
{
    // Cuerpo de PUT /api/ordenes/{id}/estado
    public class ActualizarEstadoEntrada
    {
        [Required(ErrorMessage = "El estado es requerido.")]
        [EnumDataType(typeof(EstadoOrden), ErrorMessage = "El estado no es válido.")]
        public EstadoOrden? Estado { get; set; }
    }
}
