using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.Services
{
    public class NavegacionService : INavegacionService
    {
        private static Page PaginaActual =>
            Shell.Current?.CurrentPage ?? Application.Current!.Windows[0].Page!;

        public Task IrAAsync(string ruta, IDictionary<string, object>? parametros = null) =>
            parametros is null
                ? Shell.Current.GoToAsync(ruta)
                : Shell.Current.GoToAsync(ruta, parametros);

        public Task VolverAsync() => Shell.Current.GoToAsync("..");

        public Task VolverAsync(IDictionary<string, object> parametros) => Shell.Current.GoToAsync("..", parametros);

        public Task MostrarAlertaAsync(string titulo, string mensaje) =>
            PaginaActual.DisplayAlertAsync(titulo, mensaje, "Aceptar");

        public Task<bool> ConfirmarAsync(string titulo, string mensaje, string aceptar = "Sí", string cancelar = "No") =>
            PaginaActual.DisplayAlertAsync(titulo, mensaje, aceptar, cancelar);

        public Task<string?> PedirTextoAsync(string titulo, string mensaje, string? placeholder = null) =>
            PaginaActual.DisplayPromptAsync(titulo, mensaje, "Aceptar", "Cancelar", placeholder, maxLength: 300);
    }
}
