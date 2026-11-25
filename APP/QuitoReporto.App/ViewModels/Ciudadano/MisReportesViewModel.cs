using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Ciudadano
{
    // CU-06 Ver mis reportes
    public partial class MisReportesViewModel(
        IReporteService reportes,
        IAuthService auth,
        INavegacionService navegacion) : BaseViewModel(navegacion)
    {
        private CancellationTokenSource? busquedaCts;

        public ObservableCollection<Reporte> Reportes { get; } = [];

        public List<OpcionEstado> Estados { get; } = OpcionEstado.Crear();

        [ObservableProperty]
        public partial string Buscar { get; set; } = string.Empty;

        [ObservableProperty]
        public partial OpcionEstado? EstadoSeleccionado { get; set; }

        [ObservableProperty]
        public partial bool Refrescando { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoVacio))]
        public partial bool SinResultados { get; set; }

        public string TextoVacio => string.IsNullOrWhiteSpace(Buscar) && EstadoSeleccionado?.Valor is null
            ? "Aún no tienes reportes. ¡Crea el primero desde la pestaña \"Nuevo\"!"
            : "No hay reportes que coincidan con la búsqueda.";

        public string Saludo => $"Hola, {auth.UsuarioActual?.Nombre ?? "vecino"}";

        partial void OnEstadoSeleccionadoChanged(OpcionEstado? value) => CargarCommand.Execute(null);

        // Búsqueda con una pequeña espera para no llamar a la API en cada tecla
        async partial void OnBuscarChanged(string value)
        {
            busquedaCts?.Cancel();
            busquedaCts = new CancellationTokenSource();
            var token = busquedaCts.Token;
            try
            {
                await Task.Delay(400, token);
                await CargarAsync();
            }
            catch (TaskCanceledException)
            {
                // Se escribió otra letra
            }
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            await EjecutarAsync(async () =>
            {
                var lista = await reportes.ListarAsync(new FiltroReportes
                {
                    SoloMios = true,
                    Buscar = Buscar,
                    Estado = EstadoSeleccionado?.Valor
                });
                Reportes.Clear();
                foreach (var r in lista) Reportes.Add(r);
                SinResultados = Reportes.Count == 0;
            }, "No se pudieron cargar tus reportes");
            Refrescando = false;
        }

        [RelayCommand]
        private Task AbrirAsync(Reporte? reporte) =>
            reporte is null
                ? Task.CompletedTask
                : Navegacion.IrAAsync(Rutas.Detalle, new Dictionary<string, object> { ["id"] = reporte.Id });
    }
}
