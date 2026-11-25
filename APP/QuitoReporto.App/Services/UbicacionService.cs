using System.Globalization;
using System.Text.Json;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.Services
{
    /// <summary>GPS del dispositivo y geocodificación inversa con OpenStreetMap (Nominatim).</summary>
    public class UbicacionService : IUbicacionService
    {
        private readonly HttpClient nominatim;

        public UbicacionService()
        {
            nominatim = new HttpClient { BaseAddress = new Uri(Constantes.NominatimUrl), Timeout = TimeSpan.FromSeconds(10) };
            nominatim.DefaultRequestHeaders.UserAgent.ParseAdd(Constantes.OsmUserAgent);
        }

        public async Task<Location?> ObtenerPosicionAsync(CancellationToken ct = default)
        {
            var permiso = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permiso != PermissionStatus.Granted)
                permiso = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (permiso != PermissionStatus.Granted)
                return null;

            try
            {
                var ubicacion = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(15)), ct);
                return ubicacion ?? await Geolocation.Default.GetLastKnownLocationAsync();
            }
            catch (FeatureNotEnabledException)
            {
                return null; // GPS apagado
            }
            catch (FeatureNotSupportedException)
            {
                return null;
            }
        }

        public async Task<UbicacionActual> ObtenerDireccionAsync(double latitud, double longitud, CancellationToken ct = default)
        {
            var resultado = new UbicacionActual { Latitud = latitud, Longitud = longitud };
            try
            {
                var lat = latitud.ToString(CultureInfo.InvariantCulture);
                var lon = longitud.ToString(CultureInfo.InvariantCulture);
                var json = await nominatim.GetStringAsync($"/reverse?format=jsonv2&lat={lat}&lon={lon}&accept-language=es&zoom=18", ct);

                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("address", out var direccion))
                    return resultado;

                string? Campo(params string[] nombres)
                {
                    foreach (var n in nombres)
                        if (direccion.TryGetProperty(n, out var v) && v.GetString() is { Length: > 0 } s)
                            return s;
                    return null;
                }

                var calle = Campo("road", "pedestrian", "footway");
                var numero = Campo("house_number");
                var barrio = Campo("neighbourhood", "quarter", "suburb", "city_district", "village", "town");

                var partes = new[] { calle is null ? null : (numero is null ? calle : $"{calle} {numero}"), barrio }
                    .Where(p => !string.IsNullOrEmpty(p));
                resultado.Direccion = string.Join(", ", partes);
                if (string.IsNullOrEmpty(resultado.Direccion) && doc.RootElement.TryGetProperty("display_name", out var nombre))
                    resultado.Direccion = nombre.GetString();

                // La zona agrupa por parroquia o sector (más amplio que el barrio) para las estadísticas.
                resultado.Zona = Campo("suburb", "city_district", "quarter", "neighbourhood", "village", "town");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                // Sin dirección: se envían solo las coordenadas.
            }
            return resultado;
        }
    }
}
