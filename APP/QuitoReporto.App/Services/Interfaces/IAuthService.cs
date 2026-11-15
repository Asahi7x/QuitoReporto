using QuitoReporto.App.Models;

namespace QuitoReporto.App.Services.Interfaces
{
    public interface IAuthService
    {
        Usuario? UsuarioActual { get; }

        bool EstaAutenticado { get; }

        /// <summary>Restaura la sesión guardada en el dispositivo. Devuelve true si hay una sesión válida.</summary>
        Task<bool> RestaurarSesionAsync();

        Task<Usuario> IniciarSesionAsync(string correo, string clave);

        Task<Usuario> RegistrarAsync(string nombre, string correo, string clave);

        Task<string> SolicitarCodigoAsync(string correo);

        Task<string> RestablecerClaveAsync(string correo, string codigo, string nuevaClave);

        Task CerrarSesionAsync();
    }
}
