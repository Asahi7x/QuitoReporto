using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuitoReporto.Api.Data;
using QuitoReporto.Api.Dtos;
using QuitoReporto.Api.Models;
using QuitoReporto.Api.Services;

namespace QuitoReporto.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(
        AppDbContext db,
        IPasswordHasher<Usuario> hasher,
        ITokenService tokens,
        IEmailService email) : ControllerBase
    {
        private const int MinutosValidezCodigo = 15;
        private const int MaxIntentosCodigo = 5;

        // CU-01 (registro de cuenta)
        [HttpPost("registro")]
        public async Task<ActionResult<AuthRespuestaDto>> Registrar(RegistroDto dto)
        {
            var correo = NormalizarCorreo(dto.Correo);
            if (await db.Usuarios.AnyAsync(u => u.Correo == correo))
                return Conflict(new MensajeDto("Ya existe una cuenta con ese correo."));

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Correo = correo,
                Rol = Rol.Ciudadano,
                FechaCreacion = DateTime.UtcNow
            };
            usuario.ClaveHash = hasher.HashPassword(usuario, dto.Clave);
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            return Respuesta(usuario);
        }

        // CU-01 Iniciar sesión
        [HttpPost("login")]
        public async Task<ActionResult<AuthRespuestaDto>> Login(LoginDto dto)
        {
            var correo = NormalizarCorreo(dto.Correo);
            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario is null)
                return Unauthorized(new MensajeDto("Correo o clave incorrectos."));

            var resultado = hasher.VerifyHashedPassword(usuario, usuario.ClaveHash, dto.Clave);
            if (resultado == PasswordVerificationResult.Failed)
                return Unauthorized(new MensajeDto("Correo o clave incorrectos."));

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.ClaveHash = hasher.HashPassword(usuario, dto.Clave);
                await db.SaveChangesAsync();
            }

            return Respuesta(usuario);
        }

        [Authorize]
        [HttpGet("yo")]
        public async Task<ActionResult<UsuarioDto>> Yo()
        {
            var id = User.ObtenerUsuarioId();
            var usuario = await db.Usuarios.FindAsync(id);
            return usuario is null ? Unauthorized() : UsuarioDto.Desde(usuario);
        }

        /// <summary>Envía al correo un código de verificación de 6 dígitos.</summary>
        [HttpPost("recuperar")]
        public async Task<ActionResult<MensajeDto>> Recuperar(RecuperarClaveDto dto)
        {
            var correo = NormalizarCorreo(dto.Correo);
            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

            // Se responde lo mismo exista o no la cuenta, para no revelar qué correos están registrados.
            if (usuario is not null)
            {
                var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
                usuario.CodigoRecuperacionHash = hasher.HashPassword(usuario, codigo);
                usuario.CodigoRecuperacionExpira = DateTime.UtcNow.AddMinutes(MinutosValidezCodigo);
                usuario.IntentosCodigoRecuperacion = 0;
                await db.SaveChangesAsync();

                await email.EnviarAsync(usuario.Correo, "QuitoReporto - Código de verificación",
                    $"Hola {usuario.Nombre},\n\nTu código para restablecer la clave es: {codigo}\n" +
                    $"Vence en {MinutosValidezCodigo} minutos. Si no lo solicitaste, ignora este mensaje.");
            }

            return Ok(new MensajeDto("Si el correo está registrado, te enviamos un código de verificación."));
        }

        [HttpPost("restablecer")]
        public async Task<ActionResult<MensajeDto>> Restablecer(RestablecerClaveDto dto)
        {
            var correo = NormalizarCorreo(dto.Correo);
            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            var invalido = BadRequest(new MensajeDto("El código no es válido o ya venció."));

            if (usuario?.CodigoRecuperacionHash is null || usuario.CodigoRecuperacionExpira < DateTime.UtcNow)
                return invalido;

            if (usuario.IntentosCodigoRecuperacion >= MaxIntentosCodigo)
                return BadRequest(new MensajeDto("Demasiados intentos. Solicita un nuevo código."));

            var resultado = hasher.VerifyHashedPassword(usuario, usuario.CodigoRecuperacionHash, dto.Codigo.Trim());
            if (resultado == PasswordVerificationResult.Failed)
            {
                usuario.IntentosCodigoRecuperacion++;
                await db.SaveChangesAsync();
                return invalido;
            }

            usuario.ClaveHash = hasher.HashPassword(usuario, dto.NuevaClave);
            usuario.CodigoRecuperacionHash = null;
            usuario.CodigoRecuperacionExpira = null;
            usuario.IntentosCodigoRecuperacion = 0;
            await db.SaveChangesAsync();

            return Ok(new MensajeDto("Tu clave fue actualizada. Ya puedes iniciar sesión."));
        }

        private AuthRespuestaDto Respuesta(Usuario usuario)
        {
            var (token, expira) = tokens.Generar(usuario);
            return new AuthRespuestaDto(token, expira, UsuarioDto.Desde(usuario));
        }

        private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();
    }
}
