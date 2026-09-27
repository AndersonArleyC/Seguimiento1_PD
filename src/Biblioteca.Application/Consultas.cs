namespace Biblioteca.Application;

public sealed record AutorDto(Guid Id, string Nombre);
public sealed record CategoriaDto(Guid Id, string Nombre);
public sealed record LibroDto(Guid Id, string Titulo, string Isbn, int AnioPublicacion,
    AutorDto Autor, CategoriaDto Categoria);
public sealed record LibroDetalleDto(Guid Id, string Titulo, string Isbn, int AnioPublicacion,
    string Descripcion, AutorDto Autor, CategoriaDto Categoria);

// Puerto de lectura CQRS: no expone DbContext, IQueryable ni operaciones de escritura.
public interface ICatalogoLectura
{
    Task<IReadOnlyList<LibroDto>> TodosAsync(CancellationToken ct);
    Task<LibroDetalleDto?> PorIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<LibroDto>> PorCategoriaAsync(Guid categoriaId, CancellationToken ct);
}
public interface IQuery<TResult> { }
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken ct = default);
}
public sealed record ConsultarLibros : IQuery<IReadOnlyList<LibroDto>>;
public sealed record ConsultarLibroPorId(Guid Id) : IQuery<LibroDetalleDto?>;
public sealed record ConsultarLibrosPorCategoria(Guid CategoriaId) : IQuery<IReadOnlyList<LibroDto>>;

public sealed class ConsultarLibrosHandler(ICatalogoLectura catalogo)
    : IQueryHandler<ConsultarLibros, IReadOnlyList<LibroDto>>
{
    public Task<IReadOnlyList<LibroDto>> Handle(ConsultarLibros query, CancellationToken ct = default) =>
        catalogo.TodosAsync(ct);
}
public sealed class ConsultarLibroPorIdHandler(ICatalogoLectura catalogo)
    : IQueryHandler<ConsultarLibroPorId, LibroDetalleDto?>
{
    public Task<LibroDetalleDto?> Handle(ConsultarLibroPorId query, CancellationToken ct = default) =>
        catalogo.PorIdAsync(query.Id, ct);
}
public sealed class ConsultarLibrosPorCategoriaHandler(ICatalogoLectura catalogo)
    : IQueryHandler<ConsultarLibrosPorCategoria, IReadOnlyList<LibroDto>>
{
    public Task<IReadOnlyList<LibroDto>> Handle(ConsultarLibrosPorCategoria query, CancellationToken ct = default) =>
        catalogo.PorCategoriaAsync(query.CategoriaId, ct);
}

