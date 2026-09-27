using Biblioteca.Application;
using Biblioteca.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<CatalogoDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("Catalogo")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:Catalogo.")));
builder.Services.AddScoped<ICatalogoLectura, CatalogoLectura>();
builder.Services.AddScoped<IQueryHandler<ConsultarLibros, IReadOnlyList<LibroDto>>, ConsultarLibrosHandler>();
builder.Services.AddScoped<IQueryHandler<ConsultarLibroPorId, LibroDetalleDto?>, ConsultarLibroPorIdHandler>();
builder.Services.AddScoped<IQueryHandler<ConsultarLibrosPorCategoria, IReadOnlyList<LibroDto>>, ConsultarLibrosPorCategoriaHandler>();

var app = builder.Build();
app.UseExceptionHandler();
// Inicialización explícita para una base NUEVA de práctica; nunca ocurre al arrancar normalmente.
if (args.Contains("--initialize-database"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CatalogoDbContext>();
    var created = await db.Database.EnsureCreatedAsync();
    Console.WriteLine(created ? "Catálogo creado con datos de ejemplo." : "La base ya existe; no se sobrescribió.");
    return;
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/libros", async (IQueryHandler<ConsultarLibros, IReadOnlyList<LibroDto>> handler,
    CancellationToken ct) => Results.Ok(await handler.Handle(new ConsultarLibros(), ct)));
app.MapGet("/api/libros/{id:guid}", async (Guid id,
    IQueryHandler<ConsultarLibroPorId, LibroDetalleDto?> handler, CancellationToken ct) =>
{
    var libro = await handler.Handle(new ConsultarLibroPorId(id), ct);
    return libro is null ? Results.NotFound() : Results.Ok(libro);
});
app.MapGet("/api/categorias/{categoriaId:guid}/libros", async (Guid categoriaId,
    IQueryHandler<ConsultarLibrosPorCategoria, IReadOnlyList<LibroDto>> handler, CancellationToken ct) =>
    Results.Ok(await handler.Handle(new ConsultarLibrosPorCategoria(categoriaId), ct)));
app.Run();
public partial class Program { }

