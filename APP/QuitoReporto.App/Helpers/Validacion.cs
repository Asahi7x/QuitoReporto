using System.Text.RegularExpressions;

namespace QuitoReporto.App.Helpers
{
    public static partial class Validacion
    {
        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex CorreoRegex();

        public static bool EsCorreoValido(string? correo) =>
            !string.IsNullOrWhiteSpace(correo) && CorreoRegex().IsMatch(correo.Trim());

        public const int LongitudMinimaClave = 6;
    }
}
