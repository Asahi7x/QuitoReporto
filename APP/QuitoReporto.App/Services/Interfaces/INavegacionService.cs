namespace QuitoReporto.App.Services.Interfaces
{
    /// <summary>Navegación y diálogos, para que los ViewModels no dependan de Shell directamente.</summary>
    public interface INavegacionService
    {
        Task IrAAsync(string ruta, IDictionary<string, object>? parametros = null);

        Task VolverAsync();

        /// <summary>Vuelve a la pantalla anterior entregándole datos (los recibe con IQueryAttributable).</summary>
        Task VolverAsync(IDictionary<string, object> parametros);

        Task MostrarAlertaAsync(string titulo, string mensaje);

        Task<bool> ConfirmarAsync(string titulo, string mensaje, string aceptar = "Sí", string cancelar = "No");

        Task<string?> PedirTextoAsync(string titulo, string mensaje, string? placeholder = null);
    }
}
