namespace QuitoReporto.Api.Services
{
    public interface IAlmacenFotos
    {
        /// <summary>Guarda la foto y devuelve su ruta pública relativa (por ejemplo /uploads/abc.jpg).</summary>
        Task<string> GuardarAsync(IFormFile archivo, CancellationToken ct = default);
    }

    /// <summary>Guarda las fotos en una carpeta del servidor que se publica en /uploads.</summary>
    public class AlmacenFotosLocal(string carpeta) : IAlmacenFotos
    {
        public const string RutaPublica = "/uploads";

        public string Carpeta { get; } = carpeta;

        public async Task<string> GuardarAsync(IFormFile archivo, CancellationToken ct = default)
        {
            Directory.CreateDirectory(Carpeta);

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp" or ".heic"))
                extension = ".jpg";

            var nombre = $"{Guid.NewGuid():N}{extension}";
            await using var destino = File.Create(Path.Combine(Carpeta, nombre));
            await archivo.CopyToAsync(destino, ct);

            return $"{RutaPublica}/{nombre}";
        }
    }
}
