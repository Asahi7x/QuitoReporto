using Mapsui;
using Mapsui.Limiting;
using Mapsui.Projections;
using Mapsui.Tiling;

namespace QuitoReporto.App
{
    public partial class MainPage : ContentPage
    {
        const string OsmUserAgent = "QuitoReporto/1.0 (+https://github.com/Asahi7x/QuitoReporto)";

        // Centro histórico de Quito (Plaza Grande)
        const double QuitoLon = -78.5126;
        const double QuitoLat = -0.2202;

        // Límites aproximados del Distrito Metropolitano de Quito
        const double MinLon = -78.80, MinLat = -0.42;
        const double MaxLon = -78.20, MaxLat = 0.15;

        public MainPage()
        {
            InitializeComponent();

            var map = MapControl.Map;
            // OpenStreetMap bloquea ("Access blocked") las peticiones sin un User-Agent propio de la app
            map.Layers.Add(OpenStreetMap.CreateTileLayer(OsmUserAgent));

            var (minX, minY) = SphericalMercator.FromLonLat(MinLon, MinLat);
            var (maxX, maxY) = SphericalMercator.FromLonLat(MaxLon, MaxLat);
            var quitoExtent = new MRect(minX, minY, maxX, maxY);

            // Evita que el usuario se salga de Quito o aleje demasiado el mapa
            map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
            map.Navigator.OverridePanBounds = quitoExtent;
            map.Navigator.OverrideZoomBounds = new MMinMax(0.5, 150);

            var (x, y) = SphericalMercator.FromLonLat(QuitoLon, QuitoLat);
            map.Navigator.CenterOnAndZoomTo(new MPoint(x, y), 20);
        }
    }
}
