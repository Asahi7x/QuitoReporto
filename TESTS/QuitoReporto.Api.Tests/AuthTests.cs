using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using QuitoReporto.Api.Dtos;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Tests
{
    public class AuthTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory factory;

        public AuthTests(ApiFactory factory) => this.factory = factory;

        [Fact]
        public async Task Registro_DevuelveTokenYRolCiudadano()
        {
            var cliente = factory.CreateClient();

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/registro",
                new RegistroDto("Ana Pérez", "  Ana@Correo.EC ", "Clave123"));

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            var auth = await respuesta.Content.ReadFromJsonAsync<AuthRespuestaDto>();
            Assert.False(string.IsNullOrEmpty(auth!.Token));
            Assert.Equal("ana@correo.ec", auth.Usuario.Correo);
            Assert.Equal(Rol.Ciudadano, auth.Usuario.Rol);
        }

        [Fact]
        public async Task Registro_CorreoDuplicado_DevuelveConflict()
        {
            var cliente = factory.CreateClient();
            await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroDto("Luis", "luis@correo.ec", "Clave123"));

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroDto("Luis 2", "LUIS@correo.ec", "Clave123"));

            Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        }

        [Fact]
        public async Task Registro_ClaveCorta_DevuelveBadRequest()
        {
            var cliente = factory.CreateClient();

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroDto("Eva", "eva@correo.ec", "123"));

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        public async Task Login_ClaveIncorrecta_DevuelveUnauthorized()
        {
            var cliente = factory.CreateClient();
            await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroDto("Pedro", "pedro@correo.ec", "Clave123"));

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginDto("pedro@correo.ec", "otra-clave"));

            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }

        [Fact]
        public async Task Login_Administrador_TieneRolAdministrador()
        {
            var cliente = factory.CreateClient();

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginDto(ApiFactory.CorreoAdmin, ApiFactory.ClaveAdmin));

            respuesta.EnsureSuccessStatusCode();
            var auth = await respuesta.Content.ReadFromJsonAsync<AuthRespuestaDto>();
            Assert.Equal(Rol.Administrador, auth!.Usuario.Rol);
        }

        [Fact]
        public async Task Yo_SinToken_DevuelveUnauthorized()
        {
            var respuesta = await factory.CreateClient().GetAsync("/api/auth/yo");

            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }

        [Fact]
        public async Task RecuperarClave_ConCodigoDelCorreo_PermiteIniciarSesionConNuevaClave()
        {
            var cliente = factory.CreateClient();
            const string correo = "olvido@correo.ec";
            await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroDto("Olvidadizo", correo, "ClaveVieja1"));

            var solicitud = await cliente.PostAsJsonAsync("/api/auth/recuperar", new RecuperarClaveDto(correo));
            solicitud.EnsureSuccessStatusCode();

            var cuerpo = factory.Correos.Enviados.Last(e => e.Destinatario == correo).Cuerpo;
            var codigo = Regex.Match(cuerpo, @"\b\d{6}\b").Value;

            var restablecer = await cliente.PostAsJsonAsync("/api/auth/restablecer", new RestablecerClaveDto(correo, codigo, "ClaveNueva1"));
            Assert.Equal(HttpStatusCode.OK, restablecer.StatusCode);

            var loginViejo = await cliente.PostAsJsonAsync("/api/auth/login", new LoginDto(correo, "ClaveVieja1"));
            var loginNuevo = await cliente.PostAsJsonAsync("/api/auth/login", new LoginDto(correo, "ClaveNueva1"));
            Assert.Equal(HttpStatusCode.Unauthorized, loginViejo.StatusCode);
            Assert.Equal(HttpStatusCode.OK, loginNuevo.StatusCode);

            // El código solo sirve una vez
            var reutilizar = await cliente.PostAsJsonAsync("/api/auth/restablecer", new RestablecerClaveDto(correo, codigo, "OtraClave1"));
            Assert.Equal(HttpStatusCode.BadRequest, reutilizar.StatusCode);
        }

        [Fact]
        public async Task RecuperarClave_CodigoIncorrecto_DevuelveBadRequest()
        {
            var cliente = factory.CreateClient();
            const string correo = "codigo-malo@correo.ec";
            await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroDto("Usuario", correo, "Clave123"));
            await cliente.PostAsJsonAsync("/api/auth/recuperar", new RecuperarClaveDto(correo));

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/restablecer", new RestablecerClaveDto(correo, "abcdef", "ClaveNueva1"));

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        }

        [Fact]
        public async Task RecuperarClave_CorreoInexistente_RespondeIgualSinEnviarCorreo()
        {
            var cliente = factory.CreateClient();
            var antes = factory.Correos.Enviados.Count;

            var respuesta = await cliente.PostAsJsonAsync("/api/auth/recuperar", new RecuperarClaveDto("nadie@correo.ec"));

            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
            Assert.DoesNotContain(factory.Correos.Enviados.Skip(antes), e => e.Destinatario == "nadie@correo.ec");
        }
    }
}
