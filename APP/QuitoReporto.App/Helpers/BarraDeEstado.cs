using CommunityToolkit.Maui.Core;
using QuitoReporto.App.Views.Auth;

namespace QuitoReporto.App.Helpers
{
    /// <summary>
    /// Pinta la barra de estado del sistema según la pantalla: azul noche en el inicio de sesión
    /// (iconos claros) y del color de la superficie en el resto (iconos oscuros en modo claro).
    /// </summary>
    public static class BarraDeEstado
    {
        public static void Ajustar(Page? pagina)
        {
#if ANDROID || IOS
            try
            {
                var oscuro = Application.Current?.RequestedTheme == AppTheme.Dark;
                if (pagina is LoginPage)
                {
                    CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Recurso("Ink"));
                    CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.LightContent);
                }
                else
                {
                    CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Recurso(oscuro ? "SurfaceDark" : "Surface"));
                    CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(oscuro ? StatusBarStyle.LightContent : StatusBarStyle.DarkContent);
                }
            }
            catch
            {
                // La barra de estado es solo estética: nunca debe interrumpir la navegación
            }
#endif
        }

        private static Color Recurso(string clave) =>
            Application.Current?.Resources.TryGetValue(clave, out var valor) == true && valor is Color c ? c : Colors.White;
    }
}
