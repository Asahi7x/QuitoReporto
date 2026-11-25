using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Services
{
    public class JwtOpciones
    {
        public string Emisor { get; set; } = "QuitoReporto.Api";
        public string Audiencia { get; set; } = "QuitoReporto.App";
        public string Clave { get; set; } = string.Empty;
        public int HorasValidez { get; set; } = 24 * 7;

        public SymmetricSecurityKey ObtenerLlave() => new(Encoding.UTF8.GetBytes(Clave));
    }

    public interface ITokenService
    {
        (string Token, DateTime Expira) Generar(Usuario usuario);
    }

    public class TokenService(JwtOpciones opciones) : ITokenService
    {
        public (string Token, DateTime Expira) Generar(Usuario usuario)
        {
            var expira = DateTime.UtcNow.AddHours(opciones.HorasValidez);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, usuario.Nombre),
                    new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                    new Claim("role", usuario.Rol.ToString())
                ]),
                Expires = expira,
                Issuer = opciones.Emisor,
                Audience = opciones.Audiencia,
                SigningCredentials = new SigningCredentials(opciones.ObtenerLlave(), SecurityAlgorithms.HmacSha256)
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
        }
    }
}
