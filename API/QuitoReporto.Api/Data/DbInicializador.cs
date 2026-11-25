using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Data
{
    /// <summary>Aplica las migraciones pendientes y crea el administrador inicial.</summary>
    public static class DbInicializador
    {
        public static async Task InicializarAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

            if (db.Database.IsRelational())
            {
                try
                {
                    await db.Database.MigrateAsync();
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    throw new InvalidOperationException(
                        "No se pudo conectar a PostgreSQL. ¿Está levantada la base de datos? " +
                        "Ejecuta 'docker compose up -d' en la raíz del repositorio y revisa " +
                        "ConnectionStrings:Default en appsettings.json.", ex);
                }
            }
            else
                await db.Database.EnsureCreatedAsync();

            var correoAdmin = (config["Admin:Correo"] ?? "admin@quitoreporto.ec").Trim().ToLowerInvariant();
            var claveAdmin = config["Admin:Clave"] ?? "Admin123!";

            if (!await db.Usuarios.AnyAsync(u => u.Correo == correoAdmin))
            {
                var admin = new Usuario
                {
                    Nombre = config["Admin:Nombre"] ?? "Administrador",
                    Correo = correoAdmin,
                    Rol = Rol.Administrador,
                    FechaCreacion = DateTime.UtcNow
                };
                admin.ClaveHash = hasher.HashPassword(admin, claveAdmin);
                db.Usuarios.Add(admin);
                await db.SaveChangesAsync();
            }
        }
    }
}
