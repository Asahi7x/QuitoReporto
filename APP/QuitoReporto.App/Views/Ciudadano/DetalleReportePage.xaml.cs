using QuitoReporto.App.ViewModels.Ciudadano;

namespace QuitoReporto.App.Views.Ciudadano
{
    public partial class DetalleReportePage : ContentPage
    {
        public DetalleReportePage(DetalleReporteViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
