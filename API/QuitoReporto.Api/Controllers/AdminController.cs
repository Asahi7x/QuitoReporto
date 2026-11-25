using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuitoReporto.Api.Data;
using QuitoReporto.Api.Dtos;
using QuitoReporto.Api.Models;

namespace QuitoReporto.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = nameof(Rol.Administrador))]
    [Route("api/admin")]
    public class AdminController(AppDbContext db) : ControllerBase
    {
        // CU-10 Ver estadísticas
        [HttpGet("estadisticas")]
        public async Task<EstadisticasDto> Estadisticas()
        {
            var porEstado = await db.Reportes
                .GroupBy(r => r.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            var conteoCategorias = await db.Reportes
                .GroupBy(r => r.CategoriaId)
                .Select(g => new { CategoriaId = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(g => g.CategoriaId, g => g.Cantidad);
            var categorias = await db.Categorias.OrderBy(c => c.Id).ToListAsync();
            var porCategoria = categorias
                .Select(c => new ConteoDto(c.Nombre, conteoCategorias.GetValueOrDefault(c.Id)))
                .ToList();

            var conteoZonas = await db.Reportes
                .GroupBy(r => r.Zona)
                .Select(g => new { Zona = g.Key, Cantidad = g.Count() })
                .ToListAsync();
            var porZona = conteoZonas
                .Select(z => new ConteoDto(z.Zona ?? "Sin zona", z.Cantidad))
                .OrderByDescending(c => c.Cantidad)
                .ThenBy(c => c.Nombre)
                .Take(10)
                .ToList();

            int Contar(EstadoReporte e) => porEstado.FirstOrDefault(p => p.Estado == e)?.Cantidad ?? 0;

            return new EstadisticasDto(
                porEstado.Sum(p => p.Cantidad),
                Contar(EstadoReporte.Recibido),
                Contar(EstadoReporte.EnProceso),
                Contar(EstadoReporte.Resuelto),
                porCategoria,
                porZona);
        }
    }
}
