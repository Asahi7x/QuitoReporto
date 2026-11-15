namespace QuitoReporto.App.Models
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Texto { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        public string FechaTexto => Fecha.ToLocalTime().ToString("d MMM · HH:mm", Cultura.Es);
        public string Inicial => string.IsNullOrEmpty(Autor) ? "?" : Autor[..1].ToUpperInvariant();
    }
}
