using QuitoReporto.App.Helpers;
using QuitoReporto.App.Views.Auth;
using QuitoReporto.App.Views.Ciudadano;

namespace QuitoReporto.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Pantallas que se abren encima de las pestañas
            Routing.RegisterRoute(Rutas.Registro, typeof(RegistroPage));
            Routing.RegisterRoute(Rutas.RecuperarClave, typeof(RecuperarClavePage));
            Routing.RegisterRoute(Rutas.Detalle, typeof(DetalleReportePage));
        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            BarraDeEstado.Ajustar(CurrentPage);
        }
    }
}
