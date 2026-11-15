using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Auth
{
    // CU-01 Iniciar sesión
    public partial class LoginViewModel(IAuthService auth, INavegacionService navegacion) : BaseViewModel(navegacion)
    {
        private bool sesionVerificada;

        [ObservableProperty]
        public partial string Correo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Clave { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string? Error { get; set; }

        /// <summary>Al abrir la app, entra directo si hay una sesión guardada.</summary>
        [RelayCommand]
        private async Task VerificarSesionAsync()
        {
            if (sesionVerificada) return;
            sesionVerificada = true;

            await EjecutarAsync(async () =>
            {
                if (await auth.RestaurarSesionAsync())
                    await IrAInicioAsync();
            });
        }

        [RelayCommand]
        private async Task IniciarSesionAsync()
        {
            Error = null;
            if (!Validacion.EsCorreoValido(Correo))
            {
                Error = "Ingresa un correo válido.";
                return;
            }
            if (string.IsNullOrEmpty(Clave))
            {
                Error = "Ingresa tu clave.";
                return;
            }

            await EjecutarAsync(async () =>
            {
                await auth.IniciarSesionAsync(Correo.Trim(), Clave);
                Clave = string.Empty;
                await IrAInicioAsync();
            }, "No se pudo iniciar sesión");
        }

        [RelayCommand]
        private Task IrARegistroAsync() => Navegacion.IrAAsync(Rutas.Registro);

        [RelayCommand]
        private Task IrARecuperarAsync() =>
            Navegacion.IrAAsync(Rutas.RecuperarClave, new Dictionary<string, object> { ["correo"] = Correo.Trim() });

        private Task IrAInicioAsync() =>
            Navegacion.IrAAsync(auth.UsuarioActual?.EsAdministrador == true ? Rutas.Admin : Rutas.Ciudadano);
    }
}
