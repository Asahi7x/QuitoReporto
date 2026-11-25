namespace QuitoReporto.Api.Models
{
    /// <summary>Ciclo de vida de un reporte: Recibido → En proceso → Resuelto.</summary>
    public enum EstadoReporte
    {
        Recibido = 0,
        EnProceso = 1,
        Resuelto = 2
    }

    public enum Rol
    {
        Ciudadano = 0,
        Administrador = 1
    }
}
