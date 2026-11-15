using System.Globalization;
using System.Net.Http.Headers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.Services
{
    /// <summary>Reportes, apoyos, comentarios, cambios de estado y estadísticas.</summary>
    public class ReporteService(ApiCliente api) : IReporteService
    {
        public async Task<IReadOnlyList<Reporte>> ListarAsync(FiltroReportes filtro)
        {
            var parametros = new List<string>();
            void Agregar(string clave, object? valor)
            {
                if (valor is null || valor is string s && string.IsNullOrWhiteSpace(s)) return;
                var texto = Convert.ToString(valor, CultureInfo.InvariantCulture)!;
                parametros.Add($"{clave}={Uri.EscapeDataString(texto)}");
            }

            Agregar("categoriaId", filtro.Categoria is { } c ? (int)c : null);
            Agregar("estado", filtro.Estado is { } e ? (int)e : null);
            Agregar("zona", filtro.Zona?.Trim());
            Agregar("buscar", filtro.Buscar?.Trim());
            if (filtro.SoloMios) Agregar("mios", "true");
            Agregar("lat", filtro.Latitud);
            Agregar("lon", filtro.Longitud);
            Agregar("radioKm", filtro.RadioKm);

            var ruta = parametros.Count == 0 ? "api/reportes" : $"api/reportes?{string.Join('&', parametros)}";
            var lista = await api.GetAsync<List<ReporteResumenApi>>(ruta);
            return lista.Select(Convertir).ToList();
        }

        public async Task<Reporte> ObtenerAsync(int id) =>
            Completar(await api.GetAsync<Reporte>($"api/reportes/{id}"));

        public async Task<Reporte> CrearAsync(NuevoReporte reporte)
        {
            using var form = new MultipartFormDataContent
            {
                { new StringContent(reporte.Titulo), "Titulo" },
                { new StringContent(((int)reporte.Categoria).ToString(CultureInfo.InvariantCulture)), "CategoriaId" },
                { new StringContent(reporte.Latitud.ToString(CultureInfo.InvariantCulture)), "Latitud" },
                { new StringContent(reporte.Longitud.ToString(CultureInfo.InvariantCulture)), "Longitud" }
            };
            if (!string.IsNullOrWhiteSpace(reporte.Descripcion)) form.Add(new StringContent(reporte.Descripcion), "Descripcion");
            if (!string.IsNullOrWhiteSpace(reporte.Direccion)) form.Add(new StringContent(reporte.Direccion), "Direccion");
            if (!string.IsNullOrWhiteSpace(reporte.Zona)) form.Add(new StringContent(reporte.Zona), "Zona");

            if (!string.IsNullOrEmpty(reporte.RutaFoto) && File.Exists(reporte.RutaFoto))
            {
                var foto = new StreamContent(File.OpenRead(reporte.RutaFoto));
                foto.Headers.ContentType = new MediaTypeHeaderValue(TipoImagen(reporte.RutaFoto));
                form.Add(foto, "Foto", Path.GetFileName(reporte.RutaFoto));
            }

            return Completar(await api.PostAsync<Reporte>("api/reportes", form));
        }

        public Task<ResultadoApoyo> AlternarApoyoAsync(int reporteId) =>
            api.PostAsync<ResultadoApoyo>($"api/reportes/{reporteId}/apoyo", (object?)null);

        public Task<Comentario> ComentarAsync(int reporteId, string texto) =>
            api.PostAsync<Comentario>($"api/reportes/{reporteId}/comentarios", new { texto });

        public async Task<Reporte> CambiarEstadoAsync(int reporteId, EstadoReporte estado, string? nota) =>
            Completar(await api.PutAsync<Reporte>($"api/reportes/{reporteId}/estado", new { estado, nota }));

        public async Task<Estadisticas> ObtenerEstadisticasAsync()
        {
            var stats = await api.GetAsync<Estadisticas>("api/admin/estadisticas");
            CalcularProporciones(stats.PorCategoria);
            CalcularProporciones(stats.PorZona);
            return stats;
        }

        private static void CalcularProporciones(List<Conteo> conteos)
        {
            var maximo = conteos.Count == 0 ? 0 : conteos.Max(c => c.Cantidad);
            foreach (var c in conteos)
                c.Proporcion = maximo == 0 ? 0 : (double)c.Cantidad / maximo;
        }

        private Reporte Convertir(ReporteResumenApi r) => new()
        {
            Id = r.Id,
            Titulo = r.Titulo,
            Categoria = (Categoria)r.CategoriaId,
            Estado = r.Estado,
            Latitud = r.Latitud,
            Longitud = r.Longitud,
            Direccion = r.Direccion,
            Zona = r.Zona,
            FotoUrl = api.UrlAbsoluta(r.FotoUrl),
            FechaCreacion = r.FechaCreacion,
            FechaActualizacion = r.FechaActualizacion,
            Apoyos = r.Apoyos,
            ComentariosTotal = r.Comentarios,
            Autor = r.Autor,
            EsMio = r.EsMio
        };

        private Reporte Completar(Reporte r)
        {
            r.FotoUrl = api.UrlAbsoluta(r.FotoUrl);
            r.ComentariosTotal = r.Comentarios.Count;
            return r;
        }

        private static string TipoImagen(string ruta) => Path.GetExtension(ruta).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            _ => "image/jpeg"
        };
    }
}
