namespace QuitoReporto.App.Services.Interfaces
{
    /// <summary>
    /// Inicio de sesión con huella digital (o Face ID en iOS).
    /// Las credenciales se guardan cifradas en SecureStorage y solo se leen después de
    /// que el sistema operativo confirma la huella.
    /// </summary>
    public interface IHuellaService
    {
        /// <summary>true si el usuario activó el ingreso con huella en este teléfono.</summary>
        bool Activa { get; }

        /// <summary>Correo de la cuenta asociada a la huella (para mostrarlo en el login).</summary>
        string? CorreoAsociado { get; }

        /// <summary>true si el usuario pidió activarla desde Perfil y falta confirmar su contraseña.</summary>
        bool ActivacionPendiente { get; set; }

        /// <summary>true si el teléfono tiene lector biométrico y al menos una huella registrada.</summary>
        Task<bool> DisponibleAsync();

        /// <summary>Muestra el diálogo del sistema para leer la huella. Devuelve true si se reconoció.</summary>
        Task<bool> AutenticarAsync(string motivo);

        /// <summary>Activa la huella guardando las credenciales cifradas.</summary>
        Task ActivarAsync(string correo, string clave);

        /// <summary>Lee las credenciales guardadas (llamar solo después de AutenticarAsync).</summary>
        Task<(string Correo, string Clave)?> ObtenerCredencialesAsync();

        /// <summary>Desactiva la huella y borra las credenciales del teléfono.</summary>
        void Desactivar();
    }
}
