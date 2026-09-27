# Biblioteca — catálogo de consulta

Actividad académica en C#, .NET 10, ASP.NET Core y EF Core 10 con SQL Server.
Contiene exactamente tres casos de uso CQRS de lectura. No hay POST, PUT ni DELETE.

## Arquitectura y decisiones

- Domain: Libro, Autor y Categoria son raíces de agregado con identidad. Libro
  referencia los otros agregados por ID. Isbn es un objeto de valor inmutable
  con validación ISBN-10/13. Los setters privados protegen el modelo.
- Application: contratos de consultas, handlers y DTOs; puerto ICatalogoLectura.
  No depende de EF ni ASP.NET.
- Infrastructure: DbContext, configuración de tablas y adaptador de lectura SQL.
  Consultas sin seguimiento, joins y proyecciones DTO; no carga tablas en memoria.
- API: rutas GET e inyección de dependencias (composition root).
- Tests: dominio, handlers con doble de pruebas y traducción del modelo a SQL.

Dependencias: API → Application/Infrastructure → Application → Domain.
Domain no depende de las demás capas. Se usa CQRS sin MediatR: cada Query tiene
su handler; no se implementa el lado Commands porque la actividad lo excluye.
CQRS no exige dos bases de datos ni un broker.

Supuestos: un libro tiene un autor y una categoría. El ID es Guid. El catálogo
corresponde a títulos, no a ejemplares ni préstamos. No se inventó un filtro de
disponibilidad porque el enunciado no define stock. La descripción amplía el detalle.
Un ID inexistente devuelve 404; una categoría sin coincidencias devuelve 200 y [].
Un identificador con formato inválido no coincide con la ruta y devuelve 404.

## Abrir y compilar

Requisitos: .NET SDK 10, SQL Server y, opcionalmente, Visual Studio 2026.
Abre Biblioteca.sln en Visual Studio

## Configurar SQL Server

En src/Biblioteca.API/appsettings.json se usa localhost, autenticación de Windows
y la base NUEVA BibliotecaCatalogo. Cambia Server si tu instancia tiene otro nombre.
Para credenciales, usa variables de entorno; nunca subas contraseñas a Git:

```powershell
$env:ConnectionStrings__Catalogo = "Server=TU_SERVIDOR;Database=BibliotecaCatalogo;Integrated Security=True;Encrypt=True"
```

### Preparación de una base de práctica nueva

Crea una base de datos llamada BibliotecaCatalogo y la ejecuta
```powershell
dotnet run --project src/Biblioteca.API -- --initialize-database
```

### Ejecutar

```powershell
dotnet run --project src/Biblioteca.API -- --urls http://localhost:5200
```


