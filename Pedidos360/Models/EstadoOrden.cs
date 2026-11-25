using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Models
{
    // Estados de una orden de servicio. Avanzan en este orden: Pendiente -> En reparación -> Listo -> Entregado.
    public enum EstadoOrden
    {
        [Display(Name = "Pendiente")]
        Pendiente = 1,

        [Display(Name = "En reparación")]
        EnReparacion = 2,

        [Display(Name = "Listo")]
        Listo = 3,

        [Display(Name = "Entregado")]
        Entregado = 4
    }
}
