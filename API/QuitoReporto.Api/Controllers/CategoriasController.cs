using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuitoReporto.Api.Data;
using QuitoReporto.Api.Dtos;

namespace QuitoReporto.Api.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriasController(AppDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<IReadOnlyList<CategoriaDto>> Listar() =>
            await db.Categorias
                .OrderBy(c => c.Id)
                .Select(c => new CategoriaDto(c.Id, c.Nombre, c.Descripcion))
                .ToListAsync();
    }
}
