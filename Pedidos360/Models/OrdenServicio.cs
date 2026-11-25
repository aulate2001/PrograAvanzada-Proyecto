using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Models
{
    // Orden de servicio de un equipo que trae un cliente.
    public class OrdenServicio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El cliente es requerido.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        public Cliente? Cliente { get; set; }

        [Required(ErrorMessage = "El tipo de servicio es requerido.")]
        [Display(Name = "Tipo de servicio")]
        public int TipoServicioId { get; set; }

        public TipoServicio? TipoServicio { get; set; }

        [Required(ErrorMessage = "El equipo es requerido.")]
        [StringLength(100, ErrorMessage = "El equipo no puede superar los 100 caracteres.")]
        public string Equipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del problema es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        [Display(Name = "Descripción del problema")]
        public string DescripcionProblema { get; set; } = string.Empty;

        [Range(0.01, 999999999, ErrorMessage = "El costo estimado debe ser mayor a cero.")]
        [Display(Name = "Costo estimado")]
        public decimal CostoEstimado { get; set; }

        public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;

        // Se asigna automaticamente al registrar la orden.
        [Display(Name = "Fecha de ingreso")]
        public DateTime FechaIngreso { get; set; }

        // Se asigna cuando la orden pasa a Entregado.
        [Display(Name = "Fecha de entrega")]
        public DateTime? FechaEntrega { get; set; }
    }
}
