namespace QuitoReporto.App.Services.Interfaces
{
    public interface ICamaraService
    {
        /// <summary>Abre la cámara y devuelve la ruta local de la foto, o null si se canceló.</summary>
        Task<string?> TomarFotoAsync();

        /// <summary>Permite elegir una foto de la galería y devuelve su ruta local, o null si se canceló.</summary>
        Task<string?> ElegirFotoAsync();
    }
}
