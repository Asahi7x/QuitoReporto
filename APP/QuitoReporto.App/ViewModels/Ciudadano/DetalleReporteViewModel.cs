using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Ciudadano
{
    // Detalle del reporte: línea de tiempo, CU-07 apoyar o comentar y CU-09 cambiar estado (administrador)
    public partial class DetalleReporteViewModel(
        IReporteService reportes,
        IAuthService auth,
        INavegacionService navegacion) : BaseViewModel(navegacion), IQueryAttributable
    {
        private int reporteId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Cargado), nameof(TextoApoyo), nameof(PuedeApoyar))]
        public partial Reporte? Reporte { get; set; }

        public bool Cargado => Reporte is not null;

        public ObservableCollection<Comentario> Comentarios { get; } = [];

        public ObservableCollection<CambioEstado> Historial { get; } = [];

        public bool SinComentarios => Comentarios.Count == 0;

        [ObservableProperty]
        public partial string NuevoComentario { get; set; } = string.Empty;

        public bool EsAdministrador => auth.UsuarioActual?.EsAdministrador == true;

        /// <summary>No tiene sentido apoyar el reporte propio.</summary>
        public bool PuedeApoyar => Reporte is { EsMio: false };

        public string TextoApoyo => Reporte is null
            ? string.Empty
            : $"{(Reporte.Apoyado ? "Apoyado" : "Apoyar")} · {Reporte.Apoyos}";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var valor) && int.TryParse(valor?.ToString(), out var id))
            {
                reporteId = id;
                Reporte = null;
                CargarCommand.Execute(null);
            }
        }

        [RelayCommand]
        private Task CargarAsync() => EjecutarAsync(async () =>
        {
            Mostrar(await reportes.ObtenerAsync(reporteId));
        }, "No se pudo cargar el reporte");

        [RelayCommand]
        private Task ApoyarAsync() => EjecutarAsync(async () =>
        {
            if (Reporte is null) return;
            var resultado = await reportes.AlternarApoyoAsync(Reporte.Id);
            Reporte.Apoyos = resultado.Apoyos;
            Reporte.Apoyado = resultado.Apoyado;
            OnPropertyChanged(nameof(TextoApoyo));
        }, "No se pudo registrar tu apoyo");

        [RelayCommand]
        private async Task ComentarAsync()
        {
            var texto = NuevoComentario.Trim();
            if (Reporte is null || texto.Length == 0) return;

            await EjecutarAsync(async () =>
            {
                var comentario = await reportes.ComentarAsync(Reporte.Id, texto);
                Comentarios.Add(comentario);
                NuevoComentario = string.Empty;
                OnPropertyChanged(nameof(SinComentarios));
            }, "No se pudo publicar el comentario");
        }

        [RelayCommand]
        private async Task CambiarEstadoAsync(string? estadoTexto)
        {
            if (Reporte is null || !Enum.TryParse<EstadoReporte>(estadoTexto, out var estado) || estado == Reporte.Estado)
                return;

            var nota = await Navegacion.PedirTextoAsync($"Cambiar a \"{estado.Nombre()}\"",
                "Puedes agregar una nota para el ciudadano (opcional):", "Ej.: Cuadrilla asignada");
            if (nota is null) return; // Cancelado

            await EjecutarAsync(async () =>
            {
                Mostrar(await reportes.CambiarEstadoAsync(Reporte.Id, estado, nota));
            }, "No se pudo cambiar el estado");
        }

        [RelayCommand]
        private async Task AbrirEnMapasAsync()
        {
            if (Reporte is null) return;
            await Map.Default.OpenAsync(Reporte.Latitud, Reporte.Longitud, new MapLaunchOptions { Name = Reporte.Titulo });
        }

        private void Mostrar(Reporte reporte)
        {
            Reporte = reporte;
            Comentarios.Clear();
            foreach (var c in reporte.Comentarios) Comentarios.Add(c);
            OnPropertyChanged(nameof(SinComentarios));
            Historial.Clear();
            foreach (var h in reporte.Historial.OrderByDescending(h => h.Fecha)) Historial.Add(h);
        }
    }
}
