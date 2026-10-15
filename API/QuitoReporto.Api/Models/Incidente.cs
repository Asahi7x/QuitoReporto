namespace QuitoReporto.Api.Models
{
    public class Incidente
    {
        public int Id { get; set; }

        public required string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public string? Foto { get; set; }

        public string Longitud { get; set; }

        public string Latitud { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaActualizacion { get; set; }
    }
}
