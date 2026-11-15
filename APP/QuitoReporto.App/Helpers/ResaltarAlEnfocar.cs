namespace QuitoReporto.App.Helpers
{
    /// <summary>
    /// Resalta el borde del campo (el Border contenedor) cuando el Entry/Editor/Picker recibe el foco.
    /// Uso: &lt;Entry h:ResaltarAlEnfocar.Activo="True" /&gt;
    /// </summary>
    public static class ResaltarAlEnfocar
    {
        public static readonly BindableProperty ActivoProperty =
            BindableProperty.CreateAttached("Activo", typeof(bool), typeof(ResaltarAlEnfocar), false, propertyChanged: AlCambiar);

        public static bool GetActivo(BindableObject view) => (bool)view.GetValue(ActivoProperty);
        public static void SetActivo(BindableObject view, bool value) => view.SetValue(ActivoProperty, value);

        private static void AlCambiar(BindableObject bindable, object anterior, object nuevo)
        {
            if (bindable is not VisualElement vista) return;
            vista.Focused -= AlEnfocar;
            vista.Unfocused -= AlDesenfocar;
            if (nuevo is true)
            {
                vista.Focused += AlEnfocar;
                vista.Unfocused += AlDesenfocar;
            }
        }

        private static Border? BuscarBorde(Element? e)
        {
            while (e is not null and not Border) e = e.Parent;
            return e as Border;
        }

        private static void AlEnfocar(object? sender, FocusEventArgs e)
        {
            if (BuscarBorde(sender as Element) is { } borde && Recurso("CampoBordeFoco") is Color color)
            {
                borde.Stroke = color;
                borde.StrokeThickness = 1.5;
            }
        }

        private static void AlDesenfocar(object? sender, FocusEventArgs e)
        {
            if (BuscarBorde(sender as Element) is { } borde)
            {
                // Vuelve al valor definido por el estilo "Campo"
                borde.ClearValue(Border.StrokeProperty);
                borde.ClearValue(Border.StrokeThicknessProperty);
            }
        }

        private static object? Recurso(string clave) =>
            Application.Current?.Resources.TryGetValue(clave, out var valor) == true ? valor : null;
    }
}
