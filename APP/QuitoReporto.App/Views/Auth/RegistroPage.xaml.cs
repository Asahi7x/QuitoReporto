using QuitoReporto.App.ViewModels.Auth;

namespace QuitoReporto.App.Views.Auth
{
    public partial class RegistroPage : ContentPage
    {
        public RegistroPage(RegistroViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
