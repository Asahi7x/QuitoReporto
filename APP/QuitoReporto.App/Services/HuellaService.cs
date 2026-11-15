using Plugin.Maui.Biometric;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.Services
{
    /// <summary>Huella digital con Plugin.Maui.Biometric + credenciales cifradas en SecureStorage.</summary>
    public class HuellaService(IBiometric biometria) : IHuellaService
    {
        public bool Activa => Preferences.Default.Get(Constantes.ClaveHuellaActiva, false);

        public string? CorreoAsociado => Preferences.Default.Get<string?>(Constantes.ClaveHuellaCorreo, null);

        public bool ActivacionPendiente
        {
            get => Preferences.Default.Get(Constantes.ClaveHuellaPendiente, false);
            set => Preferences.Default.Set(Constantes.ClaveHuellaPendiente, value);
        }

        public async Task<bool> DisponibleAsync()
        {
            try
            {
                return biometria.IsPlatformSupported
                    && await biometria.GetAuthenticationStatusAsync() == BiometricHwStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AutenticarAsync(string motivo)
        {
            try
            {
                var respuesta = await biometria.AuthenticateAsync(new AuthenticationRequest
                {
                    Title = "QuitoReporto",
                    Subtitle = motivo,
                    NegativeText = "Usar contraseña",
                    AuthStrength = AuthenticatorStrength.Strong,
                    AllowPasswordAuth = false
                }, CancellationToken.None);

                return respuesta.Status == BiometricResponseStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        public async Task ActivarAsync(string correo, string clave)
        {
            await SecureStorage.Default.SetAsync(Constantes.ClaveHuellaCredencial, clave);
            Preferences.Default.Set(Constantes.ClaveHuellaCorreo, correo);
            Preferences.Default.Set(Constantes.ClaveHuellaActiva, true);
            ActivacionPendiente = false;
        }

        public async Task<(string Correo, string Clave)?> ObtenerCredencialesAsync()
        {
            var correo = CorreoAsociado;
            var clave = await SecureStorage.Default.GetAsync(Constantes.ClaveHuellaCredencial);
            return string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(clave) ? null : (correo, clave);
        }

        public void Desactivar()
        {
            SecureStorage.Default.Remove(Constantes.ClaveHuellaCredencial);
            Preferences.Default.Remove(Constantes.ClaveHuellaCorreo);
            Preferences.Default.Remove(Constantes.ClaveHuellaActiva);
            ActivacionPendiente = false;
        }
    }
}
