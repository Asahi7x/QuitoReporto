using QuitoReporto.App.ViewModels.Auth;

namespace QuitoReporto.App.Views.Auth
{
    public partial class RecuperarClavePage : ContentPage
    {
        public RecuperarClavePage(RecuperarClaveViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
