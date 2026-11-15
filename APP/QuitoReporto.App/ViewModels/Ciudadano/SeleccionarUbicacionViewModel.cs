using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Ciudadano
{
    /// <summary>
    /// CU-04 (ajuste manual): el ciudadano mueve el mapa hasta que el pin central quede sobre el problema.
    /// Recibe "lat"/"lon" como punto inicial y devuelve "ubicacion" (UbicacionActual) a la pantalla anterior.
    /// </summary>
    public partial class SeleccionarUbicacionViewModel(
        IUbicacionService ubicacion,
        INavegacionService navegacion) : BaseViewModel(navegacion), IQueryAttributable
    {
        private CancellationTokenSource? busqueda;

        /// <summary>Punto inicial del mapa (posición GPS o la ubicación ya elegida).</summary>
        public double LatitudInicial { get; private set; } = Constantes.QuitoLatitud;
        public double LongitudInicial { get; private set; } = Constantes.QuitoLongitud;

        /// <summary>Punto bajo el pin (centro del mapa).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoCoordenadas))]
        public partial double Latitud { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoCoordenadas))]
        public partial double Longitud { get; set; }

        [ObservableProperty]
        public partial string TextoDireccion { get; set; } = "Mueve el mapa para elegir el punto";

        [ObservableProperty]
        public partial bool BuscandoDireccion { get; set; }

        /// <summary>true mientras el mapa se está moviendo (la dirección mostrada aún no corresponde).</summary>
        [ObservableProperty]
        public partial bool Moviendo { get; set; }

        public string TextoCoordenadas => $"{Latitud:F5}, {Longitud:F5}";

        private UbicacionActual? resultado;

        /// <summary>La vista pide centrar el mapa en este punto (botón "Mi ubicación").</summary>
        public event EventHandler<Location>? CentrarEn;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("lat", out var lat) && query.TryGetValue("lon", out var lon)
                && lat is double la && lon is double lo)
            {
                LatitudInicial = la;
                LongitudInicial = lo;
            }
            Latitud = LatitudInicial;
            Longitud = LongitudInicial;
        }

        /// <summary>La vista avisa que el mapa empezó a moverse.</summary>
        public void EmpezoAMoverse()
        {
            Moviendo = true;
            busqueda?.Cancel();
        }

        /// <summary>La vista avisa que el mapa se detuvo en un punto: se busca su dirección.</summary>
        public async Task SeDetuvoEnAsync(double latitud, double longitud)
        {
            Moviendo = false;
            Latitud = latitud;
            Longitud = longitud;
            resultado = null;

            busqueda?.Cancel();
            var cts = busqueda = new CancellationTokenSource();
            try
            {
                BuscandoDireccion = true;
                var encontrada = await ubicacion.ObtenerDireccionAsync(latitud, longitud, cts.Token);
                if (cts.IsCancellationRequested) return;
                resultado = encontrada;
                TextoDireccion = string.IsNullOrWhiteSpace(encontrada.Direccion) ? "Dirección no disponible" : encontrada.Direccion;
            }
            catch (OperationCanceledException)
            {
                // El usuario volvió a mover el mapa
            }
            catch
            {
                if (!cts.IsCancellationRequested) TextoDireccion = "Dirección no disponible";
            }
            finally
            {
                if (busqueda == cts) BuscandoDireccion = false;
            }
        }

        [RelayCommand]
        private async Task UbicarmeAsync()
        {
            var posicion = await ubicacion.ObtenerPosicionAsync();
            if (posicion is null)
            {
                await Navegacion.MostrarAlertaAsync("Ubicación no disponible", "Activa el GPS y concede el permiso de ubicación.");
                return;
            }
            CentrarEn?.Invoke(this, posicion);
        }

        [RelayCommand]
        private async Task ConfirmarAsync()
        {
            if (Moviendo) return;
            busqueda?.Cancel();

            // Si la dirección aún no llegó, se guarda el punto exacto con lo que haya
            var elegida = resultado is not null && resultado.Latitud == Latitud && resultado.Longitud == Longitud
                ? resultado
                : new UbicacionActual { Latitud = Latitud, Longitud = Longitud };

            // Las coordenadas siempre son las del pin, aunque la geocodificación redondee
            elegida.Latitud = Latitud;
            elegida.Longitud = Longitud;

            await Navegacion.VolverAsync(new Dictionary<string, object> { ["ubicacion"] = elegida });
        }
    }
}
