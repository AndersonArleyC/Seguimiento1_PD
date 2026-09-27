using Biblioteca.Application;
using Biblioteca.Domain;
using Biblioteca.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Tests;

public sealed class CatalogoTests
{
    [Theory]
    [InlineData("978-0-13-235088-4", "9780132350884")]
    [InlineData("0-8044-2957-X", "080442957X")]
    public void IsbnNormalizaYValida(string entrada, string esperado) =>
        Assert.Equal(new Isbn(esperado), new Isbn(entrada));

    [Theory]
    [InlineData("123")]
    [InlineData("9780132350885")]
    [InlineData("0000000000000")]
    public void RechazaIsbnInvalido(string entrada) =>
        Assert.Throws<ArgumentException>(() => new Isbn(entrada));

    [Fact]
    public void LibroRechazaAutorVacio() =>
        Assert.Throws<ArgumentException>(() => new Libro(Guid.NewGuid(), "Libro",
            new Isbn("9780132350884"), 2008, "Descripción", Guid.Empty, Guid.NewGuid()));

    [Fact]
    public void ModeloSqlServerYFiltroSeTraducen()
    {
        using var db = new CatalogoDbContext(new DbContextOptionsBuilder<CatalogoDbContext>()
            .UseSqlServer("Server=localhost;Database=BibliotecaModelo;Integrated Security=True").Options);
        var script = db.Database.GenerateCreateScript();
        Assert.Contains("CREATE TABLE [Libros]", script);
        Assert.Contains("FOREIGN KEY", script);
        Assert.Contains("Cien años de soledad", script);
        var adapter = new CatalogoLectura(db);
        var sql = adapter.Consulta(Guid.Empty).ToQueryString();
        Assert.Contains("INNER JOIN", sql);
        Assert.Contains("WHERE", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("SELECT", adapter.Consulta().ToQueryString());
        Assert.Contains("WHERE", adapter.Detalle(Guid.Empty).ToQueryString());
    }

    [Fact]
    public async Task TresHandlersConsultanElPuertoYPropaganCancelacion()
    {
        var fake = new CatalogoFake();
        using var cancellation = new CancellationTokenSource();
        var ct = cancellation.Token;
        var todos = await new ConsultarLibrosHandler(fake).Handle(new ConsultarLibros(), ct);
        Assert.Single(todos);
        var detalle = await new ConsultarLibroPorIdHandler(fake).Handle(new(fake.Libro.Id), ct);
        Assert.NotNull(detalle);
        Assert.Null(await new ConsultarLibroPorIdHandler(fake).Handle(new(Guid.NewGuid()), ct));
        var categoria = new ConsultarLibrosPorCategoriaHandler(fake);
        Assert.Single(await categoria.Handle(new(fake.Libro.Categoria.Id), ct));
        Assert.Empty(await categoria.Handle(new(Guid.NewGuid()), ct));
        Assert.Equal(ct, fake.Token);
    }

    private sealed class CatalogoFake : ICatalogoLectura
    {
        public LibroDto Libro { get; } = new(Guid.NewGuid(), "Libro", "9780132350884", 2008,
            new(Guid.NewGuid(), "Autor"), new(Guid.NewGuid(), "Categoría"));
        public CancellationToken Token { get; private set; }
        public Task<IReadOnlyList<LibroDto>> TodosAsync(CancellationToken ct)
        {
            Token = ct;
            return Task.FromResult<IReadOnlyList<LibroDto>>([Libro]);
        }
        public Task<LibroDetalleDto?> PorIdAsync(Guid id, CancellationToken ct)
        {
            Token = ct;
            return Task.FromResult(id == Libro.Id ? new LibroDetalleDto(Libro.Id, Libro.Titulo,
                Libro.Isbn, Libro.AnioPublicacion, "Detalle", Libro.Autor, Libro.Categoria) : null);
        }
        public Task<IReadOnlyList<LibroDto>> PorCategoriaAsync(Guid categoriaId, CancellationToken ct)
        {
            Token = ct;
            return Task.FromResult<IReadOnlyList<LibroDto>>(categoriaId == Libro.Categoria.Id ? [Libro] : []);
        }
    }
}
