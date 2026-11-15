using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuitoReporto.Api.Data;
using QuitoReporto.Api.Models;
using QuitoReporto.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Base de datos PostgreSQL. En las pruebas automatizadas (entorno "Testing") se usa una base en memoria.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>((sp, opciones) =>
        opciones.UseNpgsql(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")));
}

// Autenticación con JWT
builder.Services.AddOptions<JwtOpciones>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(o => o.Clave.Length >= 32, "Configura Jwt:Clave con al menos 32 caracteres.")
    .ValidateOnStart();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtOpciones>>().Value);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<JwtOpciones>((opciones, jwt) =>
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Emisor,
            ValidAudience = jwt.Audiencia,
            IssuerSigningKey = jwt.ObtenerLlave(),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

// Servicios de la aplicación
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<IAlmacenFotos>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var carpeta = config["Almacenamiento:CarpetaFotos"];
    return new AlmacenFotosLocal(string.IsNullOrWhiteSpace(carpeta)
        ? Path.Combine(env.ContentRootPath, "uploads")
        : Path.GetFullPath(carpeta, env.ContentRootPath));
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

await DbInicializador.InicializarAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Publica las fotos de los reportes en /uploads
if (app.Services.GetRequiredService<IAlmacenFotos>() is AlmacenFotosLocal almacen)
{
    Directory.CreateDirectory(almacen.Carpeta);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(almacen.Carpeta),
        RequestPath = AlmacenFotosLocal.RutaPublica
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { servicio = "QuitoReporto API", estado = "ok" }));

app.Run();

public partial class Program;
