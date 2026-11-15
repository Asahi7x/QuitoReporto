using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace QuitoReporto.Api.Services
{
    public static class ClaimsExtensions
    {
        public static int? ObtenerUsuarioId(this ClaimsPrincipal usuario) =>
            int.TryParse(usuario.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;
    }
}
