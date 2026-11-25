using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Services.Interfaces;

namespace QuitoReporto.App.ViewModels.Auth
{
    public partial class RegistroViewModel(IAuthService auth, INavegacionService navegacion) : BaseViewModel(navegacion)
    {
        [ObservableProperty]
        public partial string Nombre { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Correo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Clave { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ConfirmarClave { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string? Error { get; set; }

        [RelayCommand]
        private async Task RegistrarAsync()
        {
            Error = Validar();
            if (Error is not null) return;

            await EjecutarAsync(async () =>
            {
                await auth.RegistrarAsync(Nombre.Trim(), Correo.Trim(), Clave);
                await Navegacion.MostrarAlertaAsync("¡Bienvenido!", "Tu cuenta fue creada. Ya puedes reportar problemas en tu barrio.");
                await Navegacion.IrAAsync(Rutas.Ciudadano);
            }, "No se pudo crear la cuenta");
        }

        [RelayCommand]
        private Task VolverAsync() => Navegacion.VolverAsync();

        private string? Validar()
        {
            if (Nombre.Trim().Length < 2) return "Ingresa tu nombre.";
            if (!Validacion.EsCorreoValido(Correo)) return "Ingresa un correo válido.";
            if (Clave.Length < Validacion.LongitudMinimaClave) return $"La clave debe tener al menos {Validacion.LongitudMinimaClave} caracteres.";
            if (Clave != ConfirmarClave) return "Las claves no coinciden.";
            return null;
        }
    }
}
