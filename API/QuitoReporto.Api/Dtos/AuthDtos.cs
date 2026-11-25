using System.ComponentModel.DataAnnotations;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Dtos
{
    public record RegistroDto(
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        string Nombre,

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [StringLength(150)]
        string Correo,

        [Required(ErrorMessage = "La clave es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener al menos 6 caracteres.")]
        string Clave);

    public record LoginDto(
        [Required(ErrorMessage = "El correo es obligatorio.")] string Correo,
        [Required(ErrorMessage = "La clave es obligatoria.")] string Clave);

    public record RecuperarClaveDto(
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        string Correo);

    public record RestablecerClaveDto(
        [Required(ErrorMessage = "El correo es obligatorio.")] string Correo,
        [Required(ErrorMessage = "El código es obligatorio.")] string Codigo,
        [Required(ErrorMessage = "La nueva clave es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener al menos 6 caracteres.")]
        string NuevaClave);

    public record UsuarioDto(int Id, string Nombre, string Correo, Rol Rol)
    {
        public static UsuarioDto Desde(Usuario u) => new(u.Id, u.Nombre, u.Correo, u.Rol);
    }

    public record AuthRespuestaDto(string Token, DateTime Expira, UsuarioDto Usuario);

    public record MensajeDto(string Mensaje);
}
