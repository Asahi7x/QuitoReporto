using Mapsui;
using Mapsui.Layers;
using Mapsui.Limiting;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.ViewModels.Ciudadano;
using Brush = Mapsui.Styles.Brush;
using Color = Mapsui.Styles.Color;

namespace QuitoReporto.App.Views.Ciudadano
{
    /// <summary>
    /// El mapa de Mapsui no admite binding de marcadores, así que esta vista traduce
    /// la colección del ViewModel a capas del mapa. No hay lógica de negocio aquí.
    /// </summary>
    public partial class MapaPage : ContentPage
    {
        private const string CampoId = "id";

        private readonly MapaViewModel vm;
        private readonly MemoryLayer capaReportes = new("Reportes") { Style = null };
        private readonly MyLocationLayer capaUbicacion;
        private bool iniciado;

        public MapaPage(MapaViewModel vm)
        {
            InitializeComponent();
            BindingContext = this.vm = vm;

            var map = Mapa.Map;
            // OpenStreetMap bloquea ("Access blocked") las peticiones sin un User-Agent propio de la app
            map.Layers.Add(OpenStreetMap.CreateTileLayer(Constantes.OsmUserAgent));
            map.Layers.Add(capaReportes);
            capaUbicacion = new MyLocationLayer(map) { Enabled = false };
            map.Layers.Add(capaUbicacion);

            var (minX, minY) = SphericalMercator.FromLonLat(Constantes.QuitoMinLon, Constantes.QuitoMinLat);
            var (maxX, maxY) = SphericalMercator.FromLonLat(Constantes.QuitoMaxLon, Constantes.QuitoMaxLat);

            // Evita que el usuario se salga de Quito o aleje demasiado el mapa
            map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
            map.Navigator.OverridePanBounds = new MRect(minX, minY, maxX, maxY);
            map.Navigator.OverrideZoomBounds = new MMinMax(0.5, 150);
            map.Navigator.CenterOnAndZoomTo(APunto(Constantes.QuitoLatitud, Constantes.QuitoLongitud), 20);

            map.Tapped += AlTocarMapa;
            vm.ReportesActualizados += (_, _) => MainThread.BeginInvokeOnMainThread(DibujarReportes);
            vm.CentrarEnUbicacion += (_, _) => MainThread.BeginInvokeOnMainThread(CentrarEnUbicacion);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!iniciado)
            {
                iniciado = true;
                vm.IniciarCommand.Execute(null);
            }
            else
            {
                vm.CargarCommand.Execute(null);
            }
        }

        private void AlTocarMapa(object? sender, MapEventArgs e)
        {
            var info = e.GetMapInfo([capaReportes]);
            vm.SeleccionarReporte(info?.Feature?[CampoId] as int?);
        }

        private void DibujarReportes()
        {
            capaReportes.Features = vm.Reportes.Select(CrearMarcador).ToList();
            capaReportes.FeaturesWereModified();
            capaReportes.DataHasChanged();
        }

        private static PointFeature CrearMarcador(Reporte reporte)
        {
            var marcador = new PointFeature(APunto(reporte.Latitud, reporte.Longitud));
            marcador[CampoId] = reporte.Id;
            marcador.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                SymbolScale = 0.6,
                Fill = new Brush(Color.FromString(reporte.Estado.ColorHex())),
                Outline = new Pen(Color.White, 2.5)
            });
            return marcador;
        }

        private void CentrarEnUbicacion()
        {
            if (vm.MiUbicacion is not { } ubicacion) return;
            var punto = APunto(ubicacion.Latitude, ubicacion.Longitude);
            capaUbicacion.Enabled = true;
            capaUbicacion.UpdateMyLocation(punto);
            Mapa.Map.Navigator.CenterOnAndZoomTo(punto, 4, 500);
        }

        private static MPoint APunto(double latitud, double longitud)
        {
            var (x, y) = SphericalMercator.FromLonLat(longitud, latitud);
            return new MPoint(x, y);
        }
    }
}
