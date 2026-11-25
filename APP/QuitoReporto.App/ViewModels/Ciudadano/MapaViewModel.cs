using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Ciudadano
{
    // CU-05 Explorar mapa
    public partial class MapaViewModel(
        IReporteService reportes,
        IUbicacionService ubicacion,
        IAuthService auth,
        INavegacionService navegacion) : BaseViewModel(navegacion)
    {
        public ObservableCollection<Reporte> Reportes { get; } = [];

        public List<OpcionCategoria> Categorias { get; } = OpcionCategoria.Crear();

        public List<OpcionEstado> Estados { get; } = OpcionEstado.Crear();

        [ObservableProperty]
        public partial OpcionCategoria? CategoriaSeleccionada { get; set; }

        [ObservableProperty]
        public partial OpcionEstado? EstadoSeleccionado { get; set; }

        /// <summary>Si está activo, solo se muestran reportes a menos de 10 km de la persona.</summary>
        [ObservableProperty]
        public partial bool SoloCercanos { get; set; }

        [ObservableProperty]
        public partial Location? MiUbicacion { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HayReporteSeleccionado))]
        public partial Reporte? ReporteSeleccionado { get; set; }

        public bool HayReporteSeleccionado => ReporteSeleccionado is not null;

        public bool EsAdministrador => auth.UsuarioActual?.EsAdministrador == true;

        /// <summary>Se dispara cuando la lista de reportes cambia para que la vista redibuje los marcadores.</summary>
        public event EventHandler? ReportesActualizados;

        /// <summary>Se dispara cuando hay que centrar el mapa en la ubicación del usuario.</summary>
        public event EventHandler? CentrarEnUbicacion;

        partial void OnCategoriaSeleccionadaChanged(OpcionCategoria? value) => CargarCommand.Execute(null);
        partial void OnEstadoSeleccionadoChanged(OpcionEstado? value) => CargarCommand.Execute(null);
        partial void OnSoloCercanosChanged(bool value) => CargarCommand.Execute(null);

        [RelayCommand]
        private async Task IniciarAsync()
        {
            await CargarAsync();
            await UbicarmeAsync();
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            await EjecutarAsync(async () =>
            {
                var filtro = new FiltroReportes
                {
                    Categoria = CategoriaSeleccionada?.Valor,
                    Estado = EstadoSeleccionado?.Valor
                };
                if (SoloCercanos && MiUbicacion is not null)
                {
                    filtro.Latitud = MiUbicacion.Latitude;
                    filtro.Longitud = MiUbicacion.Longitude;
                    filtro.RadioKm = Constantes.RadioCercanosKm;
                }

                var lista = await reportes.ListarAsync(filtro);
                Reportes.Clear();
                foreach (var r in lista) Reportes.Add(r);

                if (ReporteSeleccionado is not null && lista.All(r => r.Id != ReporteSeleccionado.Id))
                    ReporteSeleccionado = null;

                ReportesActualizados?.Invoke(this, EventArgs.Empty);
            }, "No se pudieron cargar los reportes");
        }

        [RelayCommand]
        private async Task UbicarmeAsync()
        {
            var posicion = await ubicacion.ObtenerPosicionAsync();
            if (posicion is null)
            {
                if (SoloCercanos)
                    await Navegacion.MostrarAlertaAsync("Ubicación no disponible", "Activa el GPS y concede el permiso de ubicación para ver los reportes cercanos.");
                return;
            }

            MiUbicacion = posicion;
            CentrarEnUbicacion?.Invoke(this, EventArgs.Empty);
            if (SoloCercanos) await CargarAsync();
        }

        /// <summary>La vista llama a este método cuando se toca un marcador del mapa.</summary>
        public void SeleccionarReporte(int? id) =>
            ReporteSeleccionado = id is null ? null : Reportes.FirstOrDefault(r => r.Id == id);

        [RelayCommand]
        private void CerrarSeleccion() => ReporteSeleccionado = null;

        [RelayCommand]
        private Task VerDetalleAsync() =>
            ReporteSeleccionado is null
                ? Task.CompletedTask
                : Navegacion.IrAAsync(Rutas.Detalle, new Dictionary<string, object> { ["id"] = ReporteSeleccionado.Id });
    }
}
