using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Auth
{
    /// <summary>Recuperación de clave en dos pasos: pedir código por correo y restablecer con el código.</summary>
    public partial class RecuperarClaveViewModel(IAuthService auth, INavegacionService navegacion)
        : BaseViewModel(navegacion), IQueryAttributable
    {
        [ObservableProperty]
        public partial string Correo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Codigo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string NuevaClave { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ConfirmarClave { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PasoCorreo))]
        public partial bool CodigoEnviado { get; set; }

        public bool PasoCorreo => !CodigoEnviado;

        [ObservableProperty]
        public partial string? Error { get; set; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("correo", out var correo) && correo is string c)
                Correo = c;
            CodigoEnviado = false;
            Codigo = NuevaClave = ConfirmarClave = string.Empty;
            Error = null;
        }

        [RelayCommand]
        private async Task EnviarCodigoAsync()
        {
            Error = null;
            if (!Validacion.EsCorreoValido(Correo))
            {
                Error = "Ingresa un correo válido.";
                return;
            }

            await EjecutarAsync(async () =>
            {
                var mensaje = await auth.SolicitarCodigoAsync(Correo.Trim());
                CodigoEnviado = true;
                await Navegacion.MostrarAlertaAsync("Revisa tu correo", mensaje);
            }, "No se pudo enviar el código");
        }

        [RelayCommand]
        private async Task RestablecerAsync()
        {
            Error = null;
            if (Codigo.Trim().Length != 6) Error = "El código tiene 6 dígitos.";
            else if (NuevaClave.Length < Validacion.LongitudMinimaClave) Error = $"La clave debe tener al menos {Validacion.LongitudMinimaClave} caracteres.";
            else if (NuevaClave != ConfirmarClave) Error = "Las claves no coinciden.";
            if (Error is not null) return;

            await EjecutarAsync(async () =>
            {
                var mensaje = await auth.RestablecerClaveAsync(Correo.Trim(), Codigo.Trim(), NuevaClave);
                await Navegacion.MostrarAlertaAsync("Clave actualizada", mensaje);
                await Navegacion.VolverAsync();
            }, "No se pudo restablecer la clave");
        }

        [RelayCommand]
        private void CambiarCorreo() => CodigoEnviado = false;
    }
}
