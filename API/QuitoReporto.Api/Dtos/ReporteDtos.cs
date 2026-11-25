using System.ComponentModel.DataAnnotations;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Dtos
{
    public record CategoriaDto(int Id, string Nombre, string? Descripcion);

    /// <summary>Datos del formulario multipart para crear un reporte.</summary>
    public class CrearReporteDto
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 120 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
        public int CategoriaId { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitud fuera de rango.")]
        public double Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitud fuera de rango.")]
        public double Longitud { get; set; }

        [StringLength(300)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Zona { get; set; }

        public IFormFile? Foto { get; set; }
    }

    public record ReporteResumenDto(
        int Id,
        string Titulo,
        int CategoriaId,
        string Categoria,
        EstadoReporte Estado,
        double Latitud,
        double Longitud,
        string? Direccion,
        string? Zona,
        string? FotoUrl,
        DateTime FechaCreacion,
        DateTime FechaActualizacion,
        int Apoyos,
        int Comentarios,
        string Autor,
        bool EsMio);

    public record ComentarioDto(int Id, string Texto, string Autor, DateTime Fecha);

    public record CambioEstadoDto(EstadoReporte Estado, string? Nota, string? Responsable, DateTime Fecha);

    public record ReporteDetalleDto(
        int Id,
        string Titulo,
        string? Descripcion,
        int CategoriaId,
        string Categoria,
        EstadoReporte Estado,
        double Latitud,
        double Longitud,
        string? Direccion,
        string? Zona,
        string? FotoUrl,
        DateTime FechaCreacion,
        DateTime FechaActualizacion,
        string Autor,
        bool EsMio,
        int Apoyos,
        bool Apoyado,
        IReadOnlyList<ComentarioDto> Comentarios,
        IReadOnlyList<CambioEstadoDto> Historial);

    public record CrearComentarioDto(
        [Required(ErrorMessage = "El comentario no puede estar vacío.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El comentario no puede superar los 500 caracteres.")]
        string Texto);

    public record CambiarEstadoDto(
        [EnumDataType(typeof(EstadoReporte), ErrorMessage = "Estado no válido.")] EstadoReporte Estado,
        [StringLength(300)] string? Nota);

    public record ApoyoRespuestaDto(int Apoyos, bool Apoyado);

    public record ConteoDto(string Nombre, int Cantidad);

    public record EstadisticasDto(
        int Total,
        int Recibidos,
        int EnProceso,
        int Resueltos,
        IReadOnlyList<ConteoDto> PorCategoria,
        IReadOnlyList<ConteoDto> PorZona);
}
