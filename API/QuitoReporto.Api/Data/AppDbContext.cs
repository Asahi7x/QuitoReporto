using Microsoft.EntityFrameworkCore;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Reporte> Reportes => Set<Reporte>();
        public DbSet<Apoyo> Apoyos => Set<Apoyo>();
        public DbSet<Comentario> Comentarios => Set<Comentario>();
        public DbSet<CambioEstado> CambiosEstado => Set<CambioEstado>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(e =>
            {
                e.Property(u => u.Nombre).HasMaxLength(100);
                e.Property(u => u.Correo).HasMaxLength(150);
                e.HasIndex(u => u.Correo).IsUnique();
            });

            var fechaSemilla = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Categoria>(e =>
            {
                e.Property(c => c.Nombre).HasMaxLength(50);
                e.HasData(
                    new Categoria { Id = 1, Nombre = "Bache", Descripcion = "Huecos y daños en calles o veredas", FechaCreacion = fechaSemilla, FechaActualizacion = fechaSemilla },
                    new Categoria { Id = 2, Nombre = "Luz", Descripcion = "Luminarias dañadas o apagadas", FechaCreacion = fechaSemilla, FechaActualizacion = fechaSemilla },
                    new Categoria { Id = 3, Nombre = "Basura", Descripcion = "Acumulación de basura o escombros", FechaCreacion = fechaSemilla, FechaActualizacion = fechaSemilla },
                    new Categoria { Id = 4, Nombre = "Otros", Descripcion = "Otros problemas del espacio público", FechaCreacion = fechaSemilla, FechaActualizacion = fechaSemilla });
            });

            modelBuilder.Entity<Reporte>(e =>
            {
                e.Property(r => r.Titulo).HasMaxLength(120);
                e.Property(r => r.Descripcion).HasMaxLength(1000);
                e.Property(r => r.Direccion).HasMaxLength(300);
                e.Property(r => r.Zona).HasMaxLength(100);
                e.Property(r => r.FotoUrl).HasMaxLength(300);
                e.HasOne(r => r.Categoria).WithMany().HasForeignKey(r => r.CategoriaId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Usuario).WithMany().HasForeignKey(r => r.UsuarioId).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(r => r.Estado);
                e.HasIndex(r => r.CategoriaId);
                e.HasIndex(r => r.UsuarioId);
            });

            modelBuilder.Entity<Apoyo>(e =>
            {
                e.HasKey(a => new { a.ReporteId, a.UsuarioId });
                e.HasOne(a => a.Reporte).WithMany(r => r.Apoyos).HasForeignKey(a => a.ReporteId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(a => a.Usuario).WithMany().HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Comentario>(e =>
            {
                e.Property(c => c.Texto).HasMaxLength(500);
                e.HasOne(c => c.Reporte).WithMany(r => r.Comentarios).HasForeignKey(c => c.ReporteId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.Usuario).WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CambioEstado>(e =>
            {
                e.Property(c => c.Nota).HasMaxLength(300);
                e.HasOne(c => c.Reporte).WithMany(r => r.Historial).HasForeignKey(c => c.ReporteId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.Usuario).WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
