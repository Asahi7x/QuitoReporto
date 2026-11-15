using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Ciudadano
{
    // CU-02 Crear reporte (incluye CU-03 Tomar foto y CU-04 Obtener ubicación)
    public partial class NuevoReporteViewModel(
        IReporteService reportes,
        IUbicacionService ubicacion,
        ICamaraService camara,
        INavegacionService navegacion) : BaseViewModel(navegacion)
    {
        public List<OpcionCategoria> Categorias { get; } = OpcionCategoria.Crear(incluirTodas: false);

        [ObservableProperty]
        public partial string Titulo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Descripcion { get; set; } = string.Empty;

        [ObservableProperty]
        public partial OpcionCategoria? CategoriaSeleccionada { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TieneFoto), nameof(NoTieneFoto))]
        public partial string? RutaFoto { get; set; }

        public bool TieneFoto => !string.IsNullOrEmpty(RutaFoto);
        public bool NoTieneFoto => !TieneFoto;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TieneUbicacion), nameof(TextoUbicacion))]
        public partial UbicacionActual? Ubicacion { get; set; }

        public bool TieneUbicacion => Ubicacion is not null;

        public string TextoUbicacion => Ubicacion switch
        {
            null => "Ubicación no obtenida",
            { Direccion: { Length: > 0 } d } => d,
            var u => $"{u.Latitud:F5}, {u.Longitud:F5}"
        };

        [ObservableProperty]
        public partial bool BuscandoUbicacion { get; set; }

        [ObservableProperty]
        public partial string? Error { get; set; }

        [RelayCommand]
        private async Task IniciarAsync()
        {
            if (Ubicacion is null) await ObtenerUbicacionAsync();
        }

        [RelayCommand]
        private async Task ObtenerUbicacionAsync()
        {
            if (BuscandoUbicacion) return;
            try
            {
                BuscandoUbicacion = true;
                var posicion = await ubicacion.ObtenerPosicionAsync();
                if (posicion is null)
                {
                    await Navegacion.MostrarAlertaAsync("Ubicación no disponible",
                        "Activa el GPS y concede el permiso de ubicación para registrar dónde está el problema.");
                    return;
                }
                Ubicacion = await ubicacion.ObtenerDireccionAsync(posicion.Latitude, posicion.Longitude);
            }
            finally
            {
                BuscandoUbicacion = false;
            }
        }

        [RelayCommand]
        private Task TomarFotoAsync() => EjecutarAsync(async () =>
        {
            var ruta = await camara.TomarFotoAsync();
            if (ruta is not null) RutaFoto = ruta;
        }, "No se pudo tomar la foto");

        [RelayCommand]
        private Task ElegirFotoAsync() => EjecutarAsync(async () =>
        {
            var ruta = await camara.ElegirFotoAsync();
            if (ruta is not null) RutaFoto = ruta;
        }, "No se pudo cargar la foto");

        [RelayCommand]
        private void QuitarFoto() => RutaFoto = null;

        [RelayCommand]
        private async Task EnviarAsync()
        {
            Error = Validar();
            if (Error is not null) return;

            await EjecutarAsync(async () =>
            {
                var creado = await reportes.CrearAsync(new NuevoReporte
                {
                    Titulo = Titulo.Trim(),
                    Descripcion = Descripcion.Trim(),
                    Categoria = CategoriaSeleccionada!.Valor!.Value,
                    Latitud = Ubicacion!.Latitud,
                    Longitud = Ubicacion.Longitud,
                    Direccion = Ubicacion.Direccion,
                    Zona = Ubicacion.Zona,
                    RutaFoto = RutaFoto
                });

                Limpiar();
                await Navegacion.MostrarAlertaAsync("¡Reporte enviado!", "Gracias por ayudar a mejorar Quito. Puedes seguir su avance en \"Mis reportes\".");
                await Navegacion.IrAAsync(Rutas.MisReportes);
                await Navegacion.IrAAsync(Rutas.Detalle, new Dictionary<string, object> { ["id"] = creado.Id });
            }, "No se pudo enviar el reporte");
        }

        private string? Validar()
        {
            if (CategoriaSeleccionada?.Valor is null) return "Selecciona una categoría.";
            if (Titulo.Trim().Length < 3) return "Escribe un título de al menos 3 caracteres.";
            if (Ubicacion is null) return "Necesitamos la ubicación del problema. Toca \"Actualizar ubicación\".";
            if (!TieneFoto) return "Agrega una foto como evidencia.";
            return null;
        }

        private void Limpiar()
        {
            Titulo = Descripcion = string.Empty;
            CategoriaSeleccionada = null;
            RutaFoto = null;
            Error = null;
        }
    }
}
