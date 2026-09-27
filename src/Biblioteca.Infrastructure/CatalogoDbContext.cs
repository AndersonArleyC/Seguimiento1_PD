using Biblioteca.Domain;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Infrastructure;

public sealed class CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : DbContext(options)
{
    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Categoria> Categorias => Set<Categoria>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        var autor = model.Entity<Autor>();
        autor.ToTable("Autores"); autor.HasKey(x => x.Id);
        autor.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        var categoria = model.Entity<Categoria>();
        categoria.ToTable("Categorias"); categoria.HasKey(x => x.Id);
        categoria.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        var libro = model.Entity<Libro>();
        libro.ToTable("Libros"); libro.HasKey(x => x.Id);
        libro.Property(x => x.Titulo).HasMaxLength(250).IsRequired();
        libro.Property(x => x.Descripcion).HasMaxLength(2000).IsRequired();
        libro.Property(x => x.Isbn).HasConversion(x => x.Valor, x => new Isbn(x))
            .HasMaxLength(13).IsRequired();
        libro.HasIndex(x => x.Isbn).IsUnique();
        libro.HasOne<Autor>().WithMany().HasForeignKey(x => x.AutorId).OnDelete(DeleteBehavior.Restrict);
        libro.HasOne<Categoria>().WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);

        var autor1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var autor2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var categoria1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var categoria2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        autor.HasData(new Autor(autor1, "Gabriel García Márquez"), new Autor(autor2, "Robert C. Martin"));
        categoria.HasData(new Categoria(categoria1, "Novela"), new Categoria(categoria2, "Programación"));
        libro.HasData(
            new Libro(Guid.Parse("10000000-0000-0000-0000-000000000001"), "Cien años de soledad",
                new Isbn("9780307474728"), 1967, "Historia de la familia Buendía en Macondo.", autor1, categoria1),
            new Libro(Guid.Parse("10000000-0000-0000-0000-000000000002"), "Clean Code",
                new Isbn("9780132350884"), 2008, "Principios y prácticas para escribir código mantenible.", autor2, categoria2),
            new Libro(Guid.Parse("10000000-0000-0000-0000-000000000003"), "Clean Architecture",
                new Isbn("9780134494166"), 2017, "Principios de diseño y arquitectura de software.", autor2, categoria2));
    }
}

