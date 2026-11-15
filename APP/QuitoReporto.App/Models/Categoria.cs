using QuitoReporto.App.Helpers;

namespace QuitoReporto.App.Models
{
    /// <summary>Categorías de reporte. Los valores coinciden con los Id de la tabla Categorias de la API.</summary>
    public enum Categoria
    {
        Bache = 1,
        Luz = 2,
        Basura = 3,
        Otros = 4
    }

    public static class CategoriaExtensions
    {
        public static readonly IReadOnlyList<Categoria> Todas = [Categoria.Bache, Categoria.Luz, Categoria.Basura, Categoria.Otros];

        public static string Nombre(this Categoria categoria) => categoria switch
        {
            Categoria.Bache => "Bache",
            Categoria.Luz => "Luminaria",
            Categoria.Basura => "Basura",
            _ => "Otros"
        };

        /// <summary>Glifo de la fuente de iconos (usar con FontFamily="Icons").</summary>
        public static string Icono(this Categoria categoria) => categoria switch
        {
            Categoria.Bache => Iconos.Bache,
            Categoria.Luz => Iconos.Luz,
            Categoria.Basura => Iconos.Basura,
            _ => Iconos.Otros
        };
    }
}
