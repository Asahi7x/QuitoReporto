using QuitoReporto.Api.Services;

namespace QuitoReporto.Api.Tests
{
    public class GeoTests
    {
        [Fact]
        public void DistanciaKm_MismoPunto_EsCero()
        {
            Assert.Equal(0, Geo.DistanciaKm(-0.22, -78.51, -0.22, -78.51), 6);
        }

        [Fact]
        public void DistanciaKm_QuitoGuayaquil_EsAproximadamente270Km()
        {
            var distancia = Geo.DistanciaKm(-0.2202, -78.5126, -2.1894, -79.8891);

            Assert.InRange(distancia, 260, 280);
        }
    }
}
