namespace Biblioteca.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    protected Entity() { }
    protected Entity(Guid id) => Id = id != Guid.Empty ? id : throw new ArgumentException("Id obligatorio.");
}
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }
}
public sealed class Autor : AggregateRoot
{
    public string Nombre { get; private set; } = null!;
    private Autor() { }
    public Autor(Guid id, string nombre) : base(id) => Nombre = Texto.Validar(nombre, 150);
}
public sealed class Categoria : AggregateRoot
{
    public string Nombre { get; private set; } = null!;
    private Categoria() { }
    public Categoria(Guid id, string nombre) : base(id) => Nombre = Texto.Validar(nombre, 100);
}
// Objeto de valor: igualdad por contenido y validación de dígito de control.
public sealed record Isbn
{
    public string Valor { get; }
    public Isbn(string valor)
    {
        ArgumentNullException.ThrowIfNull(valor);
        var normalizado = valor.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        var valido = false;
        if (normalizado.Length == 13 && normalizado.All(char.IsAsciiDigit))
        {
            var suma = normalizado.Select((c, i) => (c - '0') * (i % 2 == 0 ? 1 : 3)).Sum();
            valido = (normalizado.StartsWith("978") || normalizado.StartsWith("979")) && suma % 10 == 0;
        }
        if (normalizado.Length == 10 && normalizado[..9].All(char.IsAsciiDigit)
            && (char.IsAsciiDigit(normalizado[9]) || normalizado[9] == 'X'))
        {
            var suma = normalizado.Select((c, i) => (c == 'X' ? 10 : c - '0') * (10 - i)).Sum();
            valido = suma % 11 == 0;
        }
        Valor = valido ? normalizado : throw new ArgumentException("ISBN-10 o ISBN-13 inválido.");
    }
    public override string ToString() => Valor;
}
public sealed class Libro : AggregateRoot
{
    public string Titulo { get; private set; } = null!;
    public Isbn Isbn { get; private set; } = null!;
    public int AnioPublicacion { get; private set; }
    public string Descripcion { get; private set; } = null!;
    public Guid AutorId { get; private set; }
    public Guid CategoriaId { get; private set; }
    private Libro() { }
    public Libro(Guid id, string titulo, Isbn isbn, int anioPublicacion,
        string descripcion, Guid autorId, Guid categoriaId) : base(id)
    {
        if (anioPublicacion is < 1 or > 9999) throw new ArgumentException("Año inválido.");
        if (autorId == Guid.Empty || categoriaId == Guid.Empty) throw new ArgumentException("Autor y categoría obligatorios.");
        Titulo = Texto.Validar(titulo, 250);
        Isbn = isbn ?? throw new ArgumentNullException(nameof(isbn));
        AnioPublicacion = anioPublicacion;
        Descripcion = Texto.Validar(descripcion, 2000);
        AutorId = autorId;
        CategoriaId = categoriaId;
    }
}
internal static class Texto
{
    public static string Validar(string texto, int maximo) =>
        !string.IsNullOrWhiteSpace(texto) && texto.Trim().Length <= maximo
            ? texto.Trim() : throw new ArgumentException($"Texto obligatorio de hasta {maximo} caracteres.");
}

