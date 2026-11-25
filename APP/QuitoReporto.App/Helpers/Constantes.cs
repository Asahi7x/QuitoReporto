namespace QuitoReporto.App.Helpers
{
    public static class Constantes
    {
        /// <summary>
        /// IP local de la PC donde corre la API (ver con "ipconfig", adaptador Wi-Fi).
        /// El celular y la PC deben estar conectados a la misma red Wi-Fi.
        /// </summary>
        public const string IpPc = "192.168.18.4";

        public const int PuertoApi = 5184;

        /// <summary>
        /// Dirección de la API REST:
        /// - Emulador de Android: 10.0.2.2 (alias de la PC dentro del emulador).
        /// - Celular físico (Android o iPhone): la IP local de la PC.
        /// - Windows / Mac: localhost.
        /// La API debe escuchar en todas las interfaces (perfil "http" de launchSettings: http://0.0.0.0:5184).
        /// </summary>
        public static string ApiUrl { get; } =
            DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.DeviceType == DeviceType.Virtual ? $"http://10.0.2.2:{PuertoApi}"
            : DeviceInfo.DeviceType == DeviceType.Physical && (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS) ? $"http://{IpPc}:{PuertoApi}"
            : $"http://localhost:{PuertoApi}";

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
        public const string ClaveHuellaActiva = "huella_activa";
        public const string ClaveHuellaCorreo = "huella_correo";
        public const string ClaveHuellaCredencial = "huella_credencial";
        public const string ClaveHuellaPendiente = "huella_pendiente";
        public const string ClaveHuellaOfrecida = "huella_ofrecida_";
    }
}
