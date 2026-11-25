using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuitoReporto.Api.Data;
using QuitoReporto.Api.Dtos;
using QuitoReporto.Api.Models;
using QuitoReporto.Api.Services;

namespace QuitoReporto.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/reportes")]
    public class ReportesController(AppDbContext db, IAlmacenFotos fotos) : ControllerBase
    {
        public const long TamanoMaximoFoto = 10 * 1024 * 1024;

        /// <summary>
        /// Lista reportes con filtros opcionales. Usado por el mapa (CU-05), "Mis reportes" (CU-06)
        /// y la revisión del administrador (CU-08).
        /// </summary>
        [HttpGet]
        public async Task<IReadOnlyList<ReporteResumenDto>> Listar(
            [FromQuery] int? categoriaId,
            [FromQuery] EstadoReporte? estado,
            [FromQuery] string? zona,
            [FromQuery] string? buscar,
            [FromQuery] bool mios = false,
            [FromQuery] double? lat = null,
            [FromQuery] double? lon = null,
            [FromQuery] double? radioKm = null)
        {
            var usuarioId = User.ObtenerUsuarioId() ?? 0;
            var consulta = db.Reportes.AsNoTracking();

            if (mios) consulta = consulta.Where(r => r.UsuarioId == usuarioId);
            if (categoriaId is not null) consulta = consulta.Where(r => r.CategoriaId == categoriaId);
            if (estado is not null) consulta = consulta.Where(r => r.Estado == estado);
            if (!string.IsNullOrWhiteSpace(zona))
            {
                var z = zona.Trim().ToLower();
                consulta = consulta.Where(r => r.Zona != null && r.Zona.ToLower().Contains(z));
            }
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var b = buscar.Trim().ToLower();
                consulta = consulta.Where(r =>
                    r.Titulo.ToLower().Contains(b) ||
                    (r.Descripcion != null && r.Descripcion.ToLower().Contains(b)) ||
                    (r.Direccion != null && r.Direccion.ToLower().Contains(b)));
            }

            var reportes = await consulta
                .OrderByDescending(r => r.FechaCreacion)
                .Select(r => new ReporteResumenDto(
                    r.Id, r.Titulo, r.CategoriaId, r.Categoria!.Nombre, r.Estado,
                    r.Latitud, r.Longitud, r.Direccion, r.Zona, r.FotoUrl,
                    r.FechaCreacion, r.FechaActualizacion,
                    r.Apoyos.Count, r.Comentarios.Count, r.Usuario!.Nombre,
                    r.UsuarioId == usuarioId))
                .ToListAsync();

            if (lat is not null && lon is not null && radioKm is > 0)
                reportes = reportes.Where(r => Geo.DistanciaKm(lat.Value, lon.Value, r.Latitud, r.Longitud) <= radioKm).ToList();

            return reportes;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReporteDetalleDto>> Obtener(int id)
        {
            var detalle = await ConsultarDetalle(id);
            return detalle is null ? NotFound(new MensajeDto("El reporte no existe.")) : detalle;
        }

        // CU-02 Crear reporte (incluye CU-03 foto y CU-04 ubicación)
        [HttpPost]
        [RequestSizeLimit(TamanoMaximoFoto + 1024 * 1024)]
        public async Task<ActionResult<ReporteDetalleDto>> Crear([FromForm] CrearReporteDto dto, CancellationToken ct)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            if (!await db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId, ct))
                return BadRequest(new MensajeDto("La categoría no existe."));

            string? fotoUrl = null;
            if (dto.Foto is { Length: > 0 })
            {
                if (dto.Foto.Length > TamanoMaximoFoto)
                    return BadRequest(new MensajeDto("La foto no puede pesar más de 10 MB."));
                if (!string.IsNullOrEmpty(dto.Foto.ContentType) && !dto.Foto.ContentType.StartsWith("image/"))
                    return BadRequest(new MensajeDto("El archivo adjunto debe ser una imagen."));

                fotoUrl = await fotos.GuardarAsync(dto.Foto, ct);
            }

            var ahora = DateTime.UtcNow;
            var reporte = new Reporte
            {
                Titulo = dto.Titulo.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                CategoriaId = dto.CategoriaId,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                Direccion = dto.Direccion?.Trim(),
                Zona = string.IsNullOrWhiteSpace(dto.Zona) ? null : dto.Zona.Trim(),
                FotoUrl = fotoUrl,
                UsuarioId = usuarioId.Value,
                Estado = EstadoReporte.Recibido,
                FechaCreacion = ahora,
                FechaActualizacion = ahora,
                Historial = [new CambioEstado { Estado = EstadoReporte.Recibido, Nota = "Reporte recibido", Fecha = ahora }]
            };
            db.Reportes.Add(reporte);
            await db.SaveChangesAsync(ct);

            var detalle = await ConsultarDetalle(reporte.Id);
            return CreatedAtAction(nameof(Obtener), new { id = reporte.Id }, detalle);
        }

        // CU-07 Apoyar: alterna el apoyo del usuario actual
        [HttpPost("{id:int}/apoyo")]
        public async Task<ActionResult<ApoyoRespuestaDto>> AlternarApoyo(int id)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();
            if (!await db.Reportes.AnyAsync(r => r.Id == id))
                return NotFound(new MensajeDto("El reporte no existe."));

            var existente = await db.Apoyos.FindAsync(id, usuarioId.Value);
            if (existente is null)
                db.Apoyos.Add(new Apoyo { ReporteId = id, UsuarioId = usuarioId.Value, Fecha = DateTime.UtcNow });
            else
                db.Apoyos.Remove(existente);
            await db.SaveChangesAsync();

            var total = await db.Apoyos.CountAsync(a => a.ReporteId == id);
            return new ApoyoRespuestaDto(total, existente is null);
        }

        // CU-07 Comentar
        [HttpPost("{id:int}/comentarios")]
        public async Task<ActionResult<ComentarioDto>> Comentar(int id, CrearComentarioDto dto)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();
            if (string.IsNullOrWhiteSpace(dto.Texto))
                return BadRequest(new MensajeDto("El comentario no puede estar vacío."));
            if (!await db.Reportes.AnyAsync(r => r.Id == id))
                return NotFound(new MensajeDto("El reporte no existe."));

            var comentario = new Comentario
            {
                ReporteId = id,
                UsuarioId = usuarioId.Value,
                Texto = dto.Texto.Trim(),
                Fecha = DateTime.UtcNow
            };
            db.Comentarios.Add(comentario);
            await db.SaveChangesAsync();

            var autor = await db.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Nombre).FirstAsync();
            return new ComentarioDto(comentario.Id, comentario.Texto, autor, comentario.Fecha);
        }

        // CU-09 Cambiar estado (solo administrador)
        [Authorize(Roles = nameof(Rol.Administrador))]
        [HttpPut("{id:int}/estado")]
        public async Task<ActionResult<ReporteDetalleDto>> CambiarEstado(int id, CambiarEstadoDto dto)
        {
            var reporte = await db.Reportes.FindAsync(id);
            if (reporte is null) return NotFound(new MensajeDto("El reporte no existe."));
            if (reporte.Estado == dto.Estado)
                return BadRequest(new MensajeDto("El reporte ya tiene ese estado."));

            var ahora = DateTime.UtcNow;
            reporte.Estado = dto.Estado;
            reporte.FechaActualizacion = ahora;
            db.CambiosEstado.Add(new CambioEstado
            {
                ReporteId = id,
                Estado = dto.Estado,
                Nota = string.IsNullOrWhiteSpace(dto.Nota) ? null : dto.Nota.Trim(),
                UsuarioId = User.ObtenerUsuarioId(),
                Fecha = ahora
            });
            await db.SaveChangesAsync();

            return (await ConsultarDetalle(id))!;
        }

        private async Task<ReporteDetalleDto?> ConsultarDetalle(int id)
        {
            var usuarioId = User.ObtenerUsuarioId() ?? 0;
            var r = await db.Reportes.AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    Reporte = r,
                    Categoria = r.Categoria!.Nombre,
                    Autor = r.Usuario!.Nombre,
                    Apoyos = r.Apoyos.Count,
                    Apoyado = r.Apoyos.Any(a => a.UsuarioId == usuarioId),
                    Comentarios = r.Comentarios
                        .OrderBy(c => c.Fecha)
                        .Select(c => new ComentarioDto(c.Id, c.Texto, c.Usuario!.Nombre, c.Fecha))
                        .ToList(),
                    Historial = r.Historial
                        .OrderBy(h => h.Fecha)
                        .Select(h => new CambioEstadoDto(h.Estado, h.Nota, h.Usuario != null ? h.Usuario.Nombre : null, h.Fecha))
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (r is null) return null;
            var rep = r.Reporte;
            return new ReporteDetalleDto(
                rep.Id, rep.Titulo, rep.Descripcion, rep.CategoriaId, r.Categoria, rep.Estado,
                rep.Latitud, rep.Longitud, rep.Direccion, rep.Zona, rep.FotoUrl,
                rep.FechaCreacion, rep.FechaActualizacion, r.Autor, rep.UsuarioId == usuarioId,
                r.Apoyos, r.Apoyado, r.Comentarios, r.Historial);
        }
    }
}
