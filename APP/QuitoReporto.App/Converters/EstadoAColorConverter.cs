using System.Globalization;
using QuitoReporto.App.Models;

namespace QuitoReporto.App.Converters
{
    internal static class Tema
    {
        public static bool Oscuro => Application.Current?.RequestedTheme == AppTheme.Dark;
    }

    /// <summary>Color sólido del estado (marcadores, puntos de la línea de tiempo).</summary>
    public class EstadoAColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is EstadoReporte estado ? Color.FromArgb(estado.ColorHex()) : Colors.Gray;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    /// <summary>Fondo suave de la etiqueta de estado, según el tema.</summary>
    public class EstadoAFondoConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is EstadoReporte estado ? Color.FromArgb(estado.FondoHex(Tema.Oscuro)) : Colors.Transparent;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    /// <summary>Color del texto de la etiqueta de estado, según el tema.</summary>
    public class EstadoATextoConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is EstadoReporte estado ? Color.FromArgb(estado.TextoHex(Tema.Oscuro)) : Colors.Gray;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    public class BoolInversoConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is not true;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is not true;
    }

    /// <summary>true si el valor no es null ni una cadena vacía (para mostrar mensajes de error).</summary>
    public class NoVacioConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is string s ? !string.IsNullOrWhiteSpace(s) : value is not null;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
