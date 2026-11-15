using QuitoReporto.App.ViewModels.Ciudadano;

namespace QuitoReporto.App.Views.Ciudadano
{
    public partial class NuevoReportePage : ContentPage
    {
        private readonly NuevoReporteViewModel vm;

        public NuevoReportePage(NuevoReporteViewModel vm)
        {
            InitializeComponent();
            BindingContext = this.vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            vm.IniciarCommand.Execute(null);
        }
    }
}
