# Interfaz del catálogo

La API ahora sirve una interfaz en la ruta raíz (/).
No agregamos operaciones de escritura ni modificamos la base de datos.

1. Detén la ejecución anterior con Ctrl+C en la terminal o Shift+F5 en Visual Studio.
2. Desde esta carpeta ejecuta:

```powershell
dotnet run --project .\src\Biblioteca.API -- --urls http://localhost:5200
```

3. Abre http://localhost:5200 en Chrome o Edge y actualiza con Ctrl+F5.

Conserva la configuración de conexión SQL que ya estabas usando. Si era una
variable de entorno temporal, ejecuta la aplicación en la misma terminal.

Funciones:
- Listado: GET /api/libros.
- Categoría: GET /api/categorias/{id}/libros.
- Detalle y consulta directa por ID: GET /api/libros/{id}.
- Filtros adicionales de título/ISBN/autor y año en el navegador.
- Estados de carga, catálogo vacío, sin coincidencias y error de conexión.

Las categorías disponibles en el selector se obtienen de los libros del catálogo.
No se inventan datos cuando falla SQL Server. Los bloques de color son portadas
tipográficas decorativas, no imágenes de las ediciones reales.
