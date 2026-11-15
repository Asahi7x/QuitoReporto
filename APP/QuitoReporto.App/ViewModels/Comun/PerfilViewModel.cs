using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Models;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Comun
{
    /// <summary>Perfil del usuario (ciudadano o administrador): datos de la cuenta, resumen y cierre de sesión.</summary>
    public partial class PerfilViewModel(
        IAuthService auth,
        IReporteService reportes,
        INavegacionService navegacion) : BaseViewModel(navegacion)
    {
        [ObservableProperty]
        public partial string Nombre { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Correo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Inicial { get; set; } = "?";

        [ObservableProperty]
        public partial string RolTexto { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool EsCiudadano { get; set; }

        // Resumen de los reportes del ciudadano
        [ObservableProperty]
        public partial int Total { get; set; }

        [ObservableProperty]
        public partial int EnCurso { get; set; }

        [ObservableProperty]
        public partial int Resueltos { get; set; }

        public string Version => $"QuitoReporto v{AppInfo.Current.VersionString}";

        [RelayCommand]
        private async Task CargarAsync()
        {
            var usuario = auth.UsuarioActual;
            if (usuario is null) return;

            Nombre = usuario.Nombre;
            Correo = usuario.Correo;
            Inicial = string.IsNullOrWhiteSpace(usuario.Nombre) ? "?" : usuario.Nombre.Trim()[..1].ToUpperInvariant();
            RolTexto = usuario.EsAdministrador ? "Administrador" : "Ciudadano";
            EsCiudadano = !usuario.EsAdministrador;

            if (!EsCiudadano) return;

            await EjecutarAsync(async () =>
            {
                var mios = await reportes.ListarAsync(new FiltroReportes { SoloMios = true });
                Total = mios.Count;
                Resueltos = mios.Count(r => r.Estado == EstadoReporte.Resuelto);
                EnCurso = Total - Resueltos;
            }, "No se pudo cargar tu resumen");
        }

        /// <summary>Reutiliza la pantalla de recuperación: se envía un código al correo y se define la nueva clave.</summary>
        [RelayCommand]
        private Task CambiarClaveAsync() =>
            Navegacion.IrAAsync(Rutas.RecuperarClave, new Dictionary<string, object> { ["correo"] = Correo });

        [RelayCommand]
        private async Task CerrarSesionAsync()
        {
            if (!await Navegacion.ConfirmarAsync("Cerrar sesión", "¿Quieres salir de tu cuenta?", "Cerrar sesión", "Cancelar"))
                return;

            await auth.CerrarSesionAsync();
            await Navegacion.IrAAsync(Rutas.Login);
        }
    }
}
