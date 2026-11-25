using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Models
{
    // Catalogo de tipos de servicio. Se carga con datos iniciales de EF (HasData), sin CRUD propio.
    public class TipoServicio
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }
}
