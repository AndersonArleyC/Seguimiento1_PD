using Biblioteca.Application;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Infrastructure;

public sealed class CatalogoLectura(CatalogoDbContext db) : ICatalogoLectura
{
    internal IQueryable<LibroDto> Consulta(Guid? categoriaId = null) =>
        from libro in db.Libros.AsNoTracking()
        join autor in db.Autores.AsNoTracking() on libro.AutorId equals autor.Id
        join categoria in db.Categorias.AsNoTracking() on libro.CategoriaId equals categoria.Id
        where !categoriaId.HasValue || libro.CategoriaId == categoriaId.Value
        orderby libro.Titulo, libro.Id
        select new LibroDto(libro.Id, libro.Titulo, libro.Isbn.Valor, libro.AnioPublicacion,
            new AutorDto(autor.Id, autor.Nombre), new CategoriaDto(categoria.Id, categoria.Nombre));

    public async Task<IReadOnlyList<LibroDto>> TodosAsync(CancellationToken ct) =>
        await Consulta().ToListAsync(ct);
    public async Task<IReadOnlyList<LibroDto>> PorCategoriaAsync(Guid categoriaId, CancellationToken ct) =>
        await Consulta(categoriaId).ToListAsync(ct);
    public Task<LibroDetalleDto?> PorIdAsync(Guid id, CancellationToken ct) =>
        Detalle(id).SingleOrDefaultAsync(ct);

    internal IQueryable<LibroDetalleDto> Detalle(Guid id) =>
        from libro in db.Libros.AsNoTracking()
         join autor in db.Autores.AsNoTracking() on libro.AutorId equals autor.Id
         join categoria in db.Categorias.AsNoTracking() on libro.CategoriaId equals categoria.Id
         where libro.Id == id
         select new LibroDetalleDto(libro.Id, libro.Titulo, libro.Isbn.Valor, libro.AnioPublicacion,
             libro.Descripcion, new AutorDto(autor.Id, autor.Nombre),
             new CategoriaDto(categoria.Id, categoria.Nombre));
}

