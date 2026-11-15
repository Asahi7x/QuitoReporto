namespace QuitoReporto.App.Helpers
{
    public static class Constantes
    {
        /// <summary>
        /// Dirección de la API REST. El emulador de Android llega a la PC por 10.0.2.2;
        /// en un celular físico cambia esto por la IP local de la PC (por ejemplo http://192.168.1.10:5184)
        /// y arranca la API con: dotnet run --project API/QuitoReporto.Api --urls http://0.0.0.0:5184
        /// </summary>
        public static string ApiUrl { get; } = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5184"
            : "http://localhost:5184";

        /// <summary>OpenStreetMap exige un User-Agent propio de la app para los mapas y la geocodificación.</summary>
        public const string OsmUserAgent = "QuitoReporto/1.0 (+https://github.com/Asahi7x/QuitoReporto)";

        public const string NominatimUrl = "https://nominatim.openstreetmap.org";

        // Centro histórico de Quito (Plaza Grande)
        public const double QuitoLatitud = -0.2202;
        public const double QuitoLongitud = -78.5126;

        // Límites aproximados del Distrito Metropolitano de Quito
        public const double QuitoMinLon = -78.80, QuitoMinLat = -0.42;
        public const double QuitoMaxLon = -78.20, QuitoMaxLat = 0.15;

        /// <summary>Radio para mostrar "reportes cercanos" en el mapa.</summary>
        public const double RadioCercanosKm = 10;

        // Claves de almacenamiento local
        public const string ClaveToken = "auth_token";
        public const string ClaveUsuario = "auth_usuario";
    }
}
