using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Services;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Auth
{
    // CU-01 Iniciar sesión (con contraseña o con huella digital)
    public partial class LoginViewModel(IAuthService auth, IHuellaService huella, INavegacionService navegacion) : BaseViewModel(navegacion)
    {
        /// <summary>La huella se pide sola solo al abrir la app, no después de cerrar sesión.</summary>
        private static bool appRecienAbierta = true;

        private bool sesionVerificada;

        [ObservableProperty]
        public partial string Correo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Clave { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string? Error { get; set; }

        /// <summary>Muestra el botón "Ingresar con huella".</summary>
        [ObservableProperty]
        public partial bool HuellaActiva { get; set; }

        /// <summary>Al abrir la app: pide la huella si está activa; si no, entra directo con la sesión guardada.</summary>
        [RelayCommand]
        private async Task VerificarSesionAsync()
        {
            if (sesionVerificada) return;
            sesionVerificada = true;

            var abrirApp = appRecienAbierta;
            appRecienAbierta = false;

            HuellaActiva = huella.Activa;
            if (HuellaActiva)
            {
                Correo = huella.CorreoAsociado ?? string.Empty;
                if (abrirApp) await IngresarConHuellaAsync();
                return;
            }

            if (!abrirApp) return;
            await EjecutarAsync(async () =>
            {
                if (await auth.RestaurarSesionAsync())
                    await IrAInicioAsync();
            });
        }

        [RelayCommand]
        private async Task IngresarConHuellaAsync()
        {
            Error = null;
            if (Ocupado || !await huella.AutenticarAsync("Usa tu huella para ingresar"))
                return;

            await EjecutarAsync(async () =>
            {
                // Si la sesión guardada sigue vigente y es de la misma cuenta, no hace falta pedir un token nuevo
                if (await auth.RestaurarSesionAsync()
                    && string.Equals(auth.UsuarioActual?.Correo, huella.CorreoAsociado, StringComparison.OrdinalIgnoreCase))
                {
                    await IrAInicioAsync();
                    return;
                }

                if (await huella.ObtenerCredencialesAsync() is not { } credenciales)
                {
                    DesactivarHuella("No encontramos tus datos de acceso. Ingresa con tu contraseña.");
                    return;
                }

                try
                {
                    await auth.IniciarSesionAsync(credenciales.Correo, credenciales.Clave);
                }
                catch (ApiException ex) when (ex.Codigo is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
                {
                    // La contraseña cambió desde que se activó la huella
                    DesactivarHuella("Tu contraseña cambió. Ingresa con la nueva y vuelve a activar la huella.");
                    return;
                }
                await IrAInicioAsync();
            }, "No se pudo iniciar sesión");
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
                var correo = Correo.Trim();
                await auth.IniciarSesionAsync(correo, Clave);
                await OfrecerHuellaAsync(correo, Clave);
                Clave = string.Empty;
                await IrAInicioAsync();
            }, "No se pudo iniciar sesión");
        }

        /// <summary>Después de entrar con contraseña, propone activar la huella (una sola vez por cuenta).</summary>
        private async Task OfrecerHuellaAsync(string correo, string clave)
        {
            var mismaCuenta = string.Equals(huella.CorreoAsociado, correo, StringComparison.OrdinalIgnoreCase);
            if (huella.Activa && mismaCuenta)
            {
                // Mantiene actualizada la contraseña guardada
                await huella.ActivarAsync(correo, clave);
                return;
            }

            if (!await huella.DisponibleAsync())
            {
                huella.ActivacionPendiente = false;
                return;
            }

            var claveOfrecida = Constantes.ClaveHuellaOfrecida + correo.ToLowerInvariant();
            if (!huella.ActivacionPendiente)
            {
                if (Preferences.Default.Get(claveOfrecida, false)) return;
                Preferences.Default.Set(claveOfrecida, true);

                if (!await Navegacion.ConfirmarAsync("Ingreso con huella",
                        "¿Quieres usar tu huella para entrar la próxima vez sin escribir tu contraseña?", "Activar", "Ahora no"))
                    return;
            }

            if (await huella.AutenticarAsync("Confirma tu huella para activarla"))
                await huella.ActivarAsync(correo, clave);
            else
                huella.ActivacionPendiente = false;
        }

        private void DesactivarHuella(string mensaje)
        {
            huella.Desactivar();
            HuellaActiva = false;
            Error = mensaje;
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
