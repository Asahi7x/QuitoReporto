namespace QuitoReporto.App.Models
{
    /// <summary>Elemento de un Picker de filtro. Valor null significa "Todos".</summary>
    public class Opcion<T>(string texto, T? valor) where T : struct
    {
        public string Texto { get; } = texto;
        public T? Valor { get; } = valor;

        public override string ToString() => Texto;
    }

    public class OpcionCategoria(string texto, Categoria? valor) : Opcion<Categoria>(texto, valor)
    {
        /// <summary>Glifo de la categoría para las tarjetas de selección.</summary>
        public string Icono => Valor?.Icono() ?? string.Empty;

        public static List<OpcionCategoria> Crear(bool incluirTodas = true)
        {
            var lista = CategoriaExtensions.Todas.Select(c => new OpcionCategoria(c.Nombre(), c)).ToList();
            if (incluirTodas) lista.Insert(0, new OpcionCategoria("Todas las categorías", null));
            return lista;
        }
    }

    public class OpcionEstado(string texto, EstadoReporte? valor) : Opcion<EstadoReporte>(texto, valor)
    {
        public static List<OpcionEstado> Crear() =>
        [
            new("Todos los estados", null),
            .. EstadoReporteExtensions.Todos.Select(e => new OpcionEstado(e.Nombre(), e))
        ];
    }
}
