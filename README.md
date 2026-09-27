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
Abre Biblioteca.sln en Visual Studio o abre una terminal en esta carpeta:

```powershell
dotnet restore
dotnet build
dotnet test
```

## Configurar SQL Server

En src/Biblioteca.API/appsettings.json se usa localhost, autenticación de Windows
y la base NUEVA BibliotecaCatalogo. Cambia Server si tu instancia tiene otro nombre.
Para credenciales, usa variables de entorno; nunca subas contraseñas a Git:

```powershell
$env:ConnectionStrings__Catalogo = "Server=TU_SERVIDOR;Database=BibliotecaCatalogo;Integrated Security=True;Encrypt=True"
```

La conexión exige un certificado de SQL Server válido y confiable. No se desactiva
la validación TLS automáticamente. Si falla, revisa la configuración del certificado
y la instancia con el administrador. El usuario SQL de ejecución solo necesita
SELECT; la inicialización requiere permisos de creación de base/tablas.

### Preparación de una base de práctica nueva

Solo cuando hayas confirmado que BibliotecaCatalogo es una base de práctica:

```powershell
dotnet run --project src/Biblioteca.API -- --initialize-database
```

Este comando EF Core usa EnsureCreated y carga 2 autores, 2 categorías y 3 libros.
Es una herramienta explícita de preparación, no un caso de uso de escritura.
No se ejecuta al iniciar normalmente ni borra una base existente. No sirve para
actualizar esquemas existentes: esta primera versión no incluye migraciones.
Si tu catálogo ya existe con otro esquema, habrá que adaptar el mapeo antes de usarlo.
No combines EnsureCreated con migraciones sobre la misma base.

### Ejecutar

```powershell
dotnet run --project src/Biblioteca.API -- --urls http://localhost:5200
```

Abre en Chrome o Edge:

- http://localhost:5200/api/libros
- http://localhost:5200/api/libros/10000000-0000-0000-0000-000000000001
- http://localhost:5200/api/categorias/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb/libros

También puedes ejecutar las solicitudes de requests.http desde Visual Studio.
La primera devuelve 3 libros y la categoría Programación devuelve 2 con los datos
de ejemplo. No hay interfaz gráfica ni Swagger: la entrega es una API HTTP.

## Git y entrega en GitHub

El repositorio local contiene código, pruebas y documentación, sin binarios ni secretos.
Para publicarlo, crea un repositorio vacío en tu cuenta y ejecuta:

```powershell
git remote add origin https://github.com/TU_USUARIO/Biblioteca.git
git push -u origin main
```

Entrega la URL real del repositorio, no esta URL de ejemplo. La publicación y las
pruebas contra SQL Server deben verificarse antes de considerar terminada la entrega.
Las pruebas automáticas sin SQL Server no prueban conexión ni datos reales.
