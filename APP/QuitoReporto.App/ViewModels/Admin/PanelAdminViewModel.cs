using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Admin
{
    // CU-08 Revisar reportes, CU-09 Cambiar estado y CU-10 Ver estadísticas
    public partial class PanelAdminViewModel(
        IReporteService reportes,
        IAuthService auth,
        INavegacionService navegacion) : BaseViewModel(navegacion)
    {
        public ObservableCollection<Reporte> Reportes { get; } = [];

        public List<OpcionCategoria> Categorias { get; } = OpcionCategoria.Crear();

        public List<OpcionEstado> Estados { get; } = OpcionEstado.Crear();

        [ObservableProperty]
        public partial Estadisticas? Estadisticas { get; set; }

        [ObservableProperty]
        public partial OpcionCategoria? CategoriaSeleccionada { get; set; }

        [ObservableProperty]
        public partial OpcionEstado? EstadoSeleccionado { get; set; }

        [ObservableProperty]
        public partial string Zona { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool Refrescando { get; set; }

        [ObservableProperty]
        public partial bool SinResultados { get; set; }

        public string Saludo => $"Panel de {auth.UsuarioActual?.Nombre ?? "administración"}";

        partial void OnCategoriaSeleccionadaChanged(OpcionCategoria? value) => CargarReportesCommand.Execute(null);
        partial void OnEstadoSeleccionadoChanged(OpcionEstado? value) => CargarReportesCommand.Execute(null);

        [RelayCommand]
        private async Task CargarAsync()
        {
            await EjecutarAsync(async () =>
            {
                Estadisticas = await reportes.ObtenerEstadisticasAsync();
                await ConsultarReportesAsync();
            }, "No se pudo cargar el panel");
            Refrescando = false;
        }

        [RelayCommand]
        private Task CargarReportesAsync() =>
            EjecutarAsync(ConsultarReportesAsync, "No se pudieron cargar los reportes");

        [RelayCommand]
        private Task AbrirAsync(Reporte? reporte) =>
            reporte is null
                ? Task.CompletedTask
                : Navegacion.IrAAsync(Rutas.Detalle, new Dictionary<string, object> { ["id"] = reporte.Id });

        /// <summary>Avanza el reporte al siguiente estado: Recibido → En proceso → Resuelto.</summary>
        [RelayCommand]
        private async Task AvanzarEstadoAsync(Reporte? reporte)
        {
            if (reporte?.SiguienteEstado is not { } siguiente) return;
            if (!await Navegacion.ConfirmarAsync("Cambiar estado",
                    $"¿Pasar \"{reporte.Titulo}\" a {siguiente.Nombre()}?")) return;

            await EjecutarAsync(async () =>
            {
                var actualizado = await reportes.CambiarEstadoAsync(reporte.Id, siguiente, null);
                reporte.Estado = actualizado.Estado;
                Estadisticas = await reportes.ObtenerEstadisticasAsync();

                // Si hay un filtro por estado, el reporte ya no pertenece a la lista
                if (EstadoSeleccionado?.Valor is { } filtro && filtro != reporte.Estado)
                    Reportes.Remove(reporte);
                SinResultados = Reportes.Count == 0;
            }, "No se pudo cambiar el estado");
        }

        private async Task ConsultarReportesAsync()
        {
            var lista = await reportes.ListarAsync(new FiltroReportes
            {
                Categoria = CategoriaSeleccionada?.Valor,
                Estado = EstadoSeleccionado?.Valor,
                Zona = Zona
            });
            Reportes.Clear();
            foreach (var r in lista) Reportes.Add(r);
            SinResultados = Reportes.Count == 0;
        }
    }
}
