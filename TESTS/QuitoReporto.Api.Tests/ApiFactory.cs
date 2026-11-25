using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuitoReporto.Api.Data;
using QuitoReporto.Api.Dtos;
using QuitoReporto.Api.Services;

namespace QuitoReporto.Api.Tests
{
    /// <summary>Levanta la API en memoria con una base de datos InMemory propia para cada instancia.</summary>
    public class ApiFactory : WebApplicationFactory<Program>
    {
        public const string CorreoAdmin = "admin@test.ec";
        public const string ClaveAdmin = "AdminTest123";

        private readonly string nombreBase = Guid.NewGuid().ToString();

        public EmailFalso Correos { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Clave"] = "clave-de-pruebas-con-mas-de-32-caracteres-123",
                ["Admin:Correo"] = CorreoAdmin,
                ["Admin:Clave"] = ClaveAdmin
            }));
            builder.ConfigureTestServices(services =>
            {
                services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(nombreBase));
                services.AddSingleton<IEmailService>(Correos);
                services.AddSingleton<IAlmacenFotos, AlmacenFotosFalso>();
            });
        }

        public async Task<HttpClient> CrearClienteAutenticadoAsync(string correo, string clave = "Clave123", string nombre = "Ciudadano")
        {
            var cliente = CreateClient();
            var respuesta = await cliente.PostAsJsonAsync("/api/auth/registro", new RegistroDto(nombre, correo, clave));
            if (!respuesta.IsSuccessStatusCode)
                respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginDto(correo, clave));
            respuesta.EnsureSuccessStatusCode();

            var auth = await respuesta.Content.ReadFromJsonAsync<AuthRespuestaDto>();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
            return cliente;
        }

        public async Task<HttpClient> CrearClienteAdminAsync()
        {
            var cliente = CreateClient();
            var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginDto(CorreoAdmin, ClaveAdmin));
            respuesta.EnsureSuccessStatusCode();
            var auth = await respuesta.Content.ReadFromJsonAsync<AuthRespuestaDto>();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
            return cliente;
        }
    }

    public class EmailFalso : IEmailService
    {
        public List<(string Destinatario, string Asunto, string Cuerpo)> Enviados { get; } = [];

        public Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            lock (Enviados) Enviados.Add((destinatario, asunto, cuerpo));
            return Task.CompletedTask;
        }
    }

    public class AlmacenFotosFalso : IAlmacenFotos
    {
        public Task<string> GuardarAsync(IFormFile archivo, CancellationToken ct = default) =>
            Task.FromResult($"/uploads/{Guid.NewGuid():N}.jpg");
    }
}
