using QuitoReporto.App.Helpers;

namespace QuitoReporto.App.Models
{
    public enum EstadoReporte
    {
        Recibido = 0,
        EnProceso = 1,
        Resuelto = 2
    }

    public static class EstadoReporteExtensions
    {
        public static readonly IReadOnlyList<EstadoReporte> Todos = [EstadoReporte.Recibido, EstadoReporte.EnProceso, EstadoReporte.Resuelto];

        public static string Nombre(this EstadoReporte estado) => estado switch
        {
            EstadoReporte.Recibido => "Recibido",
            EstadoReporte.EnProceso => "En proceso",
            _ => "Resuelto"
        };

        /// <summary>Glifo de la fuente de iconos (usar con FontFamily="Icons").</summary>
        public static string Icono(this EstadoReporte estado) => estado switch
        {
            EstadoReporte.Recibido => Iconos.Recibido,
            EstadoReporte.EnProceso => Iconos.EnProceso,
            _ => Iconos.Resuelto
        };

        /// <summary>Color hexadecimal usado en marcadores del mapa y etiquetas de estado.</summary>
        public static string ColorHex(this EstadoReporte estado) => estado switch
        {
            EstadoReporte.Recibido => "#FFBD59",
            EstadoReporte.EnProceso => "#AC8BFF",
            _ => "#00BF63"
        };

        /// <summary>Fondo suave para etiquetas de estado (modo claro / modo oscuro).</summary>
        public static string FondoHex(this EstadoReporte estado, bool oscuro) => estado.ColorHex();

        /// <summary>Color de texto/ícono sobre el fondo suave.</summary>
        public static string TextoHex(this EstadoReporte estado, bool oscuro) => "#FFFFFF";

        /// <summary>Siguiente estado del flujo, o null si ya está resuelto.</summary>
        public static EstadoReporte? Siguiente(this EstadoReporte estado) => estado switch
        {
            EstadoReporte.Recibido => EstadoReporte.EnProceso,
            EstadoReporte.EnProceso => EstadoReporte.Resuelto,
            _ => null
        };
    }
}
