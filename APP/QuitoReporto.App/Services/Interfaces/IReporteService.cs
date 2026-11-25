using QuitoReporto.App.Models;

namespace QuitoReporto.App.Services.Interfaces
{
    public interface IReporteService
    {
        Task<IReadOnlyList<Reporte>> ListarAsync(FiltroReportes filtro);

        Task<Reporte> ObtenerAsync(int id);

        Task<Reporte> CrearAsync(NuevoReporte reporte);

        Task<ResultadoApoyo> AlternarApoyoAsync(int reporteId);

        Task<Comentario> ComentarAsync(int reporteId, string texto);

        Task<Reporte> CambiarEstadoAsync(int reporteId, EstadoReporte estado, string? nota);

        Task<Estadisticas> ObtenerEstadisticasAsync();
    }
}
