namespace QuitoReporto.App.Models
{
    public enum Rol
    {
        Ciudadano = 0,
        Administrador = 1
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public Rol Rol { get; set; }

        public bool EsAdministrador => Rol == Rol.Administrador;
    }

    public class AuthRespuesta
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
        public Usuario Usuario { get; set; } = new();
    }
}
