namespace QuitoReporto.Api.Models
{
    public class Reporte
    {
        public int Id { get; set; }

        public required string Titulo { get; set; }

        public string? Descripcion { get; set; }

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public EstadoReporte Estado { get; set; } = EstadoReporte.Recibido;

        public double Latitud { get; set; }

        public double Longitud { get; set; }

        /// <summary>Dirección legible obtenida de la API de mapas.</summary>
        public string? Direccion { get; set; }

        /// <summary>Barrio o sector de Quito (para filtros y estadísticas por zona).</summary>
        public string? Zona { get; set; }

        /// <summary>Ruta relativa de la foto, por ejemplo /uploads/abc.jpg.</summary>
        public string? FotoUrl { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaActualizacion { get; set; }

        public List<Apoyo> Apoyos { get; set; } = [];

        public List<Comentario> Comentarios { get; set; } = [];

        public List<CambioEstado> Historial { get; set; } = [];
    }

    /// <summary>Voto de un ciudadano a un reporte. Un usuario solo puede apoyar una vez cada reporte.</summary>
    public class Apoyo
    {
        public int ReporteId { get; set; }
        public Reporte? Reporte { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public DateTime Fecha { get; set; }
    }

    public class Comentario
    {
        public int Id { get; set; }

        public int ReporteId { get; set; }
        public Reporte? Reporte { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public required string Texto { get; set; }

        public DateTime Fecha { get; set; }
    }

    /// <summary>Entrada de la línea de tiempo de un reporte.</summary>
    public class CambioEstado
    {
        public int Id { get; set; }

        public int ReporteId { get; set; }
        public Reporte? Reporte { get; set; }

        public EstadoReporte Estado { get; set; }

        public string? Nota { get; set; }

        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public DateTime Fecha { get; set; }
    }
}
