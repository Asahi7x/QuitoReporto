using Mapsui;
using Mapsui.Limiting;
using Mapsui.Projections;
using Mapsui.Tiling;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.ViewModels.Ciudadano;

namespace QuitoReporto.App.Views.Ciudadano
{
    /// <summary>
    /// Mapsui no expone un evento de "el mapa dejó de moverse", así que la vista revisa el centro
    /// del mapa cada 250 ms y avisa al ViewModel cuando se movió y cuando se detuvo.
    /// </summary>
    public partial class SeleccionarUbicacionPage : ContentPage
    {
        /// <summary>Resolución (metros por píxel) con la que se ve el detalle de calles.</summary>
        private const double ResolucionCalle = 1.2;

        private static readonly TimeSpan TiempoQuieto = TimeSpan.FromMilliseconds(600);

        private readonly SeleccionarUbicacionViewModel vm;
        private IDispatcherTimer? temporizador;
        private MPoint? ultimoCentro;
        private MPoint? centroInformado;
        private DateTime ultimoMovimiento;
        private bool iniciado;

        public SeleccionarUbicacionPage(SeleccionarUbicacionViewModel vm)
        {
            InitializeComponent();
            BindingContext = this.vm = vm;

            var map = Mapa.Map;
            map.Layers.Add(OpenStreetMap.CreateTileLayer(Constantes.OsmUserAgent));

            var (minX, minY) = SphericalMercator.FromLonLat(Constantes.QuitoMinLon, Constantes.QuitoMinLat);
            var (maxX, maxY) = SphericalMercator.FromLonLat(Constantes.QuitoMaxLon, Constantes.QuitoMaxLat);
            map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
            map.Navigator.OverridePanBounds = new MRect(minX, minY, maxX, maxY);
            map.Navigator.OverrideZoomBounds = new MMinMax(0.3, 150);

            // Tocar un punto lo lleva bajo el pin
            map.Tapped += (_, e) => map.Navigator.CenterOn(e.WorldPosition, 300);

            vm.CentrarEn += (_, posicion) => MainThread.BeginInvokeOnMainThread(() =>
                map.Navigator.CenterOnAndZoomTo(APunto(posicion.Latitude, posicion.Longitude), ResolucionCalle, 500));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!iniciado)
            {
                iniciado = true;
                Mapa.Map.Navigator.CenterOnAndZoomTo(APunto(vm.LatitudInicial, vm.LongitudInicial), ResolucionCalle);
            }

            temporizador = Dispatcher.CreateTimer();
            temporizador.Interval = TimeSpan.FromMilliseconds(250);
            temporizador.Tick += RevisarCentro;
            temporizador.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            temporizador?.Stop();
            temporizador = null;
        }

        private void RevisarCentro(object? sender, EventArgs e)
        {
            var viewport = Mapa.Map.Navigator.Viewport;
            if (viewport.Width <= 0) return; // el mapa aún no se dibuja
            var centro = new MPoint(viewport.CenterX, viewport.CenterY);

            if (ultimoCentro is null || Distancia(centro, ultimoCentro) > 0.01)
            {
                ultimoCentro = centro;
                ultimoMovimiento = DateTime.UtcNow;
                if (centroInformado is not null && !vm.Moviendo) vm.EmpezoAMoverse();
                return;
            }

            // Quieto el tiempo suficiente y en un punto distinto al ya informado
            if (DateTime.UtcNow - ultimoMovimiento >= TiempoQuieto
                && (centroInformado is null || vm.Moviendo || Distancia(centro, centroInformado) > 0.01))
            {
                centroInformado = centro;
                var (lon, lat) = SphericalMercator.ToLonLat(centro.X, centro.Y);
                _ = vm.SeDetuvoEnAsync(lat, lon);
            }
        }

        private static double Distancia(MPoint a, MPoint b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private static MPoint APunto(double latitud, double longitud)
        {
            var (x, y) = SphericalMercator.FromLonLat(longitud, latitud);
            return new MPoint(x, y);
        }
    }
}
