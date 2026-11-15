using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.Services
{
    /// <summary>Captura de fotos con la cámara o la galería. Copia la imagen a la caché de la app.</summary>
    public class CamaraService : ICamaraService
    {
        public async Task<string?> TomarFotoAsync()
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                return await ElegirFotoAsync();

            var permiso = await Permissions.RequestAsync<Permissions.Camera>();
            if (permiso != PermissionStatus.Granted)
                throw new PermissionException("Necesitamos permiso para usar la cámara.");

            var foto = await MediaPicker.Default.CapturePhotoAsync(Opciones());
            return await GuardarEnCacheAsync(foto);
        }

        public async Task<string?> ElegirFotoAsync()
        {
            var fotos = await MediaPicker.Default.PickPhotosAsync(Opciones());
            return await GuardarEnCacheAsync(fotos?.FirstOrDefault());
        }

        private static MediaPickerOptions Opciones() => new()
        {
            Title = "Foto del reporte",
            SelectionLimit = 1,
            // Reduce la foto para que la subida sea rápida con datos móviles.
            MaximumWidth = 1600,
            MaximumHeight = 1600,
            CompressionQuality = 80
        };

        private static async Task<string?> GuardarEnCacheAsync(FileResult? foto)
        {
            if (foto is null) return null;

            var extension = Path.GetExtension(foto.FileName);
            var destino = Path.Combine(FileSystem.CacheDirectory, $"reporte_{DateTime.Now:yyyyMMddHHmmss}{extension}");
            await using var origen = await foto.OpenReadAsync();
            await using var archivo = File.Create(destino);
            await origen.CopyToAsync(archivo);
            return destino;
        }
    }
}
