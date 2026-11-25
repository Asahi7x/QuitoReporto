using QuitoReporto.App.ViewModels.Comun;

namespace QuitoReporto.App.Views.Comun
{
    public partial class PerfilPage : ContentPage
    {
        private readonly PerfilViewModel vm;

        public PerfilPage(PerfilViewModel vm)
        {
            InitializeComponent();
            BindingContext = this.vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Se recarga siempre: la pestaña se reutiliza si otra persona inicia sesión
            vm.CargarCommand.Execute(null);
        }
    }
}
