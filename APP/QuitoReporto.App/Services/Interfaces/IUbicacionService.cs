using QuitoReporto.App.Models;

namespace QuitoReporto.App.Services.Interfaces
{
    public interface IUbicacionService
    {
        /// <summary>Obtiene la posición GPS actual (pide permiso si hace falta). Devuelve null si no se pudo.</summary>
        Task<Location?> ObtenerPosicionAsync(CancellationToken ct = default);

        /// <summary>Convierte coordenadas en dirección y zona usando la API de mapas (OpenStreetMap Nominatim).</summary>
        Task<UbicacionActual> ObtenerDireccionAsync(double latitud, double longitud, CancellationToken ct = default);
    }
}
