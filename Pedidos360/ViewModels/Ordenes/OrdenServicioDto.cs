namespace Pedidos360.ViewModels.Ordenes
{
    // Fila de la tabla de ordenes que devuelve la Web API (RF-03).
    public class OrdenServicioDto
    {
        public int Id { get; set; }
        public string Equipo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;
        public decimal CostoEstimado { get; set; }
        public int Estado { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaEntrega { get; set; }

        // Estado al que se puede avanzar. Null si ya esta Entregado.
        public int? SiguienteEstado { get; set; }
        public string? SiguienteEstadoTexto { get; set; }
    }
}
