using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace QuitoReporto.App.Models
{
    /// <summary>Reporte tal como lo devuelve la API (listado y detalle).</summary>
    public partial class Reporte : ObservableObject
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        [JsonPropertyName("categoriaId")]
        public Categoria Categoria { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EstadoTexto), nameof(EstadoIcono), nameof(SiguienteEstado), nameof(PuedeAvanzar), nameof(TextoAvanzar))]
        public partial EstadoReporte Estado { get; set; }

        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string? Direccion { get; set; }
        public string? Zona { get; set; }

        /// <summary>URL absoluta de la foto (el servicio la completa con la dirección de la API).</summary>
        public string? FotoUrl { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public string Autor { get; set; } = string.Empty;
        public bool EsMio { get; set; }

        [ObservableProperty]
        public partial int Apoyos { get; set; }

        [ObservableProperty]
        public partial bool Apoyado { get; set; }

        public int ComentariosTotal { get; set; }

        public List<Comentario> Comentarios { get; set; } = [];
        public List<CambioEstado> Historial { get; set; } = [];

        // Propiedades de presentación
        public string CategoriaTexto => Categoria.Nombre();
        public string CategoriaIcono => Categoria.Icono();
        public string EstadoTexto => Estado.Nombre();
        public string EstadoIcono => Estado.Icono();
        public bool TieneFoto => !string.IsNullOrEmpty(FotoUrl);
        public string FechaTexto => FechaCreacion.ToLocalTime().ToString("d MMM yyyy · HH:mm", Cultura.Es);
        public string Ubicacion => string.IsNullOrWhiteSpace(Direccion)
            ? $"{Latitud:F5}, {Longitud:F5}"
            : Direccion;

        public EstadoReporte? SiguienteEstado => Estado.Siguiente();
        public bool PuedeAvanzar => SiguienteEstado is not null;
        public string TextoAvanzar => SiguienteEstado is { } s ? $"Pasar a {s.Nombre()}" : "Resuelto";
    }

    /// <summary>Forma del JSON de la API para el listado (usa "comentarios" como número).</summary>
    public class ReporteResumenApi
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public EstadoReporte Estado { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string? Direccion { get; set; }
        public string? Zona { get; set; }
        public string? FotoUrl { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public int Apoyos { get; set; }
        public int Comentarios { get; set; }
        public string Autor { get; set; } = string.Empty;
        public bool EsMio { get; set; }
    }

    internal static class Cultura
    {
        public static readonly System.Globalization.CultureInfo Es = new("es-EC");
    }

    public class CambioEstado
    {
        public EstadoReporte Estado { get; set; }
        public string? Nota { get; set; }
        public string? Responsable { get; set; }
        public DateTime Fecha { get; set; }

        public string Titulo => Estado.Nombre();
        public string FechaTexto => Fecha.ToLocalTime().ToString("d MMM yyyy · HH:mm", Cultura.Es);
        public bool TieneNota => !string.IsNullOrWhiteSpace(Nota);
    }

    /// <summary>Datos del formulario de nuevo reporte.</summary>
    public class NuevoReporte
    {
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public Categoria Categoria { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string? Direccion { get; set; }
        public string? Zona { get; set; }
        public string? RutaFoto { get; set; }
    }

    public class FiltroReportes
    {
        public Categoria? Categoria { get; set; }
        public EstadoReporte? Estado { get; set; }
        public string? Zona { get; set; }
        public string? Buscar { get; set; }
        public bool SoloMios { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public double? RadioKm { get; set; }
    }

    public class Conteo
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }

        /// <summary>Proporción 0..1 respecto al máximo, para dibujar barras.</summary>
        public double Proporcion { get; set; }
    }

    public class Estadisticas
    {
        public int Total { get; set; }
        public int Recibidos { get; set; }
        public int EnProceso { get; set; }
        public int Resueltos { get; set; }
        public List<Conteo> PorCategoria { get; set; } = [];
        public List<Conteo> PorZona { get; set; } = [];
    }

    public class ResultadoApoyo
    {
        public int Apoyos { get; set; }
        public bool Apoyado { get; set; }
    }

    public class UbicacionActual
    {
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string? Direccion { get; set; }
        public string? Zona { get; set; }
    }
}
