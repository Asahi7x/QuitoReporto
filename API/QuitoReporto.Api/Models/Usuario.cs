namespace QuitoReporto.Api.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public required string Nombre { get; set; }

        public required string Correo { get; set; }

        public string ClaveHash { get; set; } = string.Empty;

        public Rol Rol { get; set; } = Rol.Ciudadano;

        /// <summary>Hash del código de verificación enviado por correo para recuperar la clave.</summary>
        public string? CodigoRecuperacionHash { get; set; }

        public DateTime? CodigoRecuperacionExpira { get; set; }

        public int IntentosCodigoRecuperacion { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
