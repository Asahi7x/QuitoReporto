namespace QuitoReporto.Api.Services
{
    public static class Geo
    {
        private const double RadioTierraKm = 6371.0;

        /// <summary>Distancia en kilómetros entre dos coordenadas (fórmula de Haversine).</summary>
        public static double DistanciaKm(double lat1, double lon1, double lat2, double lon2)
        {
            static double ARad(double grados) => grados * Math.PI / 180.0;

            var dLat = ARad(lat2 - lat1);
            var dLon = ARad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ARad(lat1)) * Math.Cos(ARad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return RadioTierraKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
