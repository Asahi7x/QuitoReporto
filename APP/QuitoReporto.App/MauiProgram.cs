using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using QuitoReporto.App.Helpers;
using QuitoReporto.App.Services;
using QuitoReporto.App.Services.Interfaces;
using QuitoReporto.App.ViewModels.Admin;
using QuitoReporto.App.ViewModels.Auth;
using QuitoReporto.App.ViewModels.Ciudadano;
using QuitoReporto.App.ViewModels.Comun;
using QuitoReporto.App.Views.Admin;
using QuitoReporto.App.Views.Auth;
using QuitoReporto.App.Views.Ciudadano;
using QuitoReporto.App.Views.Comun;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace QuitoReporto.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Inter-Regular.ttf", "Inter");
                    fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                    fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
                    fonts.AddFont("Inter-Bold.ttf", "InterBold");
                    fonts.AddFont("Lucide.ttf", "Icons");
                    fonts.AddFont("Questrial-Regular.ttf", "Questrial");
                    fonts.AddFont("BebasNeue-Regular.ttf", "BebasNeue");
                });

            QuitarDecoracionNativaDeCampos();

            // Servicios (una sola instancia para toda la app)
            builder.Services.AddSingleton(_ => new ApiCliente(new HttpClient { BaseAddress = new Uri(Constantes.ApiUrl) }));
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IReporteService, ReporteService>();
            builder.Services.AddSingleton<IUbicacionService, UbicacionService>();
            builder.Services.AddSingleton<ICamaraService, CamaraService>();
            builder.Services.AddSingleton<INavegacionService, NavegacionService>();
            builder.Services.AddSingleton(_ => Plugin.Maui.Biometric.BiometricAuthenticationService.Default);
            builder.Services.AddSingleton<IHuellaService, HuellaService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegistroViewModel>();
            builder.Services.AddTransient<RecuperarClaveViewModel>();
            builder.Services.AddTransient<MapaViewModel>();
            builder.Services.AddTransient<NuevoReporteViewModel>();
            builder.Services.AddTransient<MisReportesViewModel>();
            builder.Services.AddTransient<DetalleReporteViewModel>();
            builder.Services.AddTransient<SeleccionarUbicacionViewModel>();
            builder.Services.AddTransient<PanelAdminViewModel>();
            builder.Services.AddTransient<PerfilViewModel>();

            // Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegistroPage>();
            builder.Services.AddTransient<RecuperarClavePage>();
            builder.Services.AddTransient<MapaPage>();
            builder.Services.AddTransient<NuevoReportePage>();
            builder.Services.AddTransient<MisReportesPage>();
            builder.Services.AddTransient<DetalleReportePage>();
            builder.Services.AddTransient<SeleccionarUbicacionPage>();
            builder.Services.AddTransient<PanelAdminPage>();
            builder.Services.AddTransient<PerfilPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        /// <summary>
        /// Los campos se dibujan dentro de un Border propio (estilo "Campo"), así que se quita
        /// el subrayado de Android y el borde de iOS para que no aparezcan dos bordes.
        /// </summary>
        private static void QuitarDecoracionNativaDeCampos()
        {
#if ANDROID
            var transparente = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SinBorde", (h, _) => h.PlatformView.BackgroundTintList = transparente);
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("SinBorde", (h, _) => h.PlatformView.BackgroundTintList = transparente);
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("SinBorde", (h, _) => h.PlatformView.BackgroundTintList = transparente);
#elif IOS || MACCATALYST
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SinBorde", (h, _) => h.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None);
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("SinBorde", (h, _) => h.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None);
#elif WINDOWS
            // Los objetos de WinUI se crean dentro de cada mapeo (en el hilo de UI, con XAML ya iniciado)
            static void SinBorde(Microsoft.UI.Xaml.Controls.Control control)
            {
                var transparente = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                control.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                control.Background = transparente;
            }
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SinBorde", (h, _) => SinBorde(h.PlatformView));
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("SinBorde", (h, _) => SinBorde(h.PlatformView));
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("SinBorde", (h, _) => SinBorde(h.PlatformView));
#endif
        }
    }
}
