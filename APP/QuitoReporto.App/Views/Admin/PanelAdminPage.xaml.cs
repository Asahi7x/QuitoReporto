using QuitoReporto.App.Models;
using QuitoReporto.App.ViewModels.Admin;

namespace QuitoReporto.App.Views.Admin
{
    public partial class PanelAdminPage : ContentPage
    {
        private readonly PanelAdminViewModel vm;

        public PanelAdminPage(PanelAdminViewModel vm)
        {
            InitializeComponent();
            BindingContext = this.vm = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Lista.SelectedItem = null;
            vm.CargarCommand.Execute(null);
        }

        /// <summary>
        /// Botón "Pasar a ..." de cada tarjeta. Se maneja aquí en lugar de un binding con RelativeSource
        /// porque ese binding fallaba con NullReferenceException mientras la lista creaba o reciclaba tarjetas.
        /// La vista solo delega el reporte de la tarjeta al comando del ViewModel.
        /// </summary>
        private void AlAvanzarEstado(object? sender, EventArgs e)
        {
            if (sender is BindableObject { BindingContext: Reporte reporte } && vm.AvanzarEstadoCommand.CanExecute(reporte))
                vm.AvanzarEstadoCommand.Execute(reporte);
        }
    }
}
