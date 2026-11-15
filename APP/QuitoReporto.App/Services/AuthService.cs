using System.Text.Json;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.Services
{
    /// <summary>Login, registro y recuperación de clave. Guarda el token en SecureStorage.</summary>
    public class AuthService(ApiCliente api) : IAuthService
    {
        public Usuario? UsuarioActual { get; private set; }

        public bool EstaAutenticado => UsuarioActual is not null;

        public async Task<bool> RestaurarSesionAsync()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync(Constantes.ClaveToken);
                var usuarioJson = Preferences.Default.Get<string?>(Constantes.ClaveUsuario, null);
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(usuarioJson))
                    return false;

                api.EstablecerToken(token);
                // Se valida el token con la API; si expiró se descarta la sesión.
                UsuarioActual = await api.GetAsync<Usuario>("api/auth/yo");
                Preferences.Default.Set(Constantes.ClaveUsuario, JsonSerializer.Serialize(UsuarioActual, ApiCliente.Json));
                return true;
            }
            catch (ApiException ex) when (ex.Codigo is not null)
            {
                await CerrarSesionAsync();
                return false;
            }
            catch (ApiException)
            {
                // Sin conexión: se usa el usuario guardado para no expulsar a la persona.
                var usuarioJson = Preferences.Default.Get<string?>(Constantes.ClaveUsuario, null);
                UsuarioActual = usuarioJson is null ? null : JsonSerializer.Deserialize<Usuario>(usuarioJson, ApiCliente.Json);
                return UsuarioActual is not null;
            }
        }

        public async Task<Usuario> IniciarSesionAsync(string correo, string clave)
        {
            var respuesta = await api.PostAsync<AuthRespuesta>("api/auth/login", new { correo, clave });
            await GuardarSesionAsync(respuesta);
            return respuesta.Usuario;
        }

        public async Task<Usuario> RegistrarAsync(string nombre, string correo, string clave)
        {
            var respuesta = await api.PostAsync<AuthRespuesta>("api/auth/registro", new { nombre, correo, clave });
            await GuardarSesionAsync(respuesta);
            return respuesta.Usuario;
        }

        public async Task<string> SolicitarCodigoAsync(string correo) =>
            (await api.PostAsync<Mensaje>("api/auth/recuperar", new { correo })).Texto;

        public async Task<string> RestablecerClaveAsync(string correo, string codigo, string nuevaClave) =>
            (await api.PostAsync<Mensaje>("api/auth/restablecer", new { correo, codigo, nuevaClave })).Texto;

        public Task CerrarSesionAsync()
        {
            UsuarioActual = null;
            api.EstablecerToken(null);
            SecureStorage.Default.Remove(Constantes.ClaveToken);
            Preferences.Default.Remove(Constantes.ClaveUsuario);
            return Task.CompletedTask;
        }

        private async Task GuardarSesionAsync(AuthRespuesta respuesta)
        {
            api.EstablecerToken(respuesta.Token);
            UsuarioActual = respuesta.Usuario;
            await SecureStorage.Default.SetAsync(Constantes.ClaveToken, respuesta.Token);
            Preferences.Default.Set(Constantes.ClaveUsuario, JsonSerializer.Serialize(respuesta.Usuario, ApiCliente.Json));
        }

        private sealed class Mensaje
        {
            [System.Text.Json.Serialization.JsonPropertyName("mensaje")]
            public string Texto { get; set; } = string.Empty;
        }
    }
}
