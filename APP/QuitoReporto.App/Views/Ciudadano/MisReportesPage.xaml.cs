using QuitoReporto.App.ViewModels.Ciudadano;

namespace QuitoReporto.App.Views.Ciudadano
{
    public partial class MisReportesPage : ContentPage
    {
        private readonly MisReportesViewModel vm;

        public MisReportesPage(MisReportesViewModel vm)
        {
            InitializeComponent();
            BindingContext = this.vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Lista.SelectedItem = null; // Permite volver a tocar el mismo reporte
            vm.CargarCommand.Execute(null);
        }
    }
}
