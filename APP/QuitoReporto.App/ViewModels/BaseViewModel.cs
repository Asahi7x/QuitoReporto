using CommunityToolkit.Mvvm.ComponentModel;
using QuitoReporto.App.Services;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels
{
    public abstract partial class BaseViewModel(INavegacionService navegacion) : ObservableObject
    {
        protected INavegacionService Navegacion { get; } = navegacion;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NoOcupado))]
        public partial bool Ocupado { get; set; }

        public bool NoOcupado => !Ocupado;

        /// <summary>Ejecuta una acción marcando la pantalla como ocupada y mostrando los errores al usuario.</summary>
        protected async Task EjecutarAsync(Func<Task> accion, string tituloError = "Algo salió mal")
        {
            if (Ocupado) return;
            try
            {
                Ocupado = true;
                await accion();
            }
            catch (ApiException ex)
            {
                await Navegacion.MostrarAlertaAsync(tituloError, ex.Message);
            }
            catch (PermissionException ex)
            {
                await Navegacion.MostrarAlertaAsync("Permiso requerido", ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await Navegacion.MostrarAlertaAsync(tituloError, "Ocurrió un error inesperado. Inténtalo de nuevo.");
            }
            finally
            {
                Ocupado = false;
            }
        }
    }
}
