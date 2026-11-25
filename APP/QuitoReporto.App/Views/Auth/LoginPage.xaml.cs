using QuitoReporto.App.ViewModels.Auth;

namespace QuitoReporto.App.Views.Auth
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel vm;

        public LoginPage(LoginViewModel vm)
        {
            InitializeComponent();
            BindingContext = this.vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            vm.VerificarSesionCommand.Execute(null);
        }

        /// <summary>Muestra u oculta la contraseña (solo presentación).</summary>
        private void AlAlternarClave(object? sender, TappedEventArgs e)
        {
            CampoClave.IsPassword = !CampoClave.IsPassword;
            IconoOjo.Text = CampoClave.IsPassword ? Helpers.Iconos.Ojo : Helpers.Iconos.OjoCerrado;
        }
    }
}
