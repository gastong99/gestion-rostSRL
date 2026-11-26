# ERP V2 — Sistema de gestión Rostagno SRL

Aplicación web de gestión interna para una empresa industrial. El proyecto se desarrolla con ASP.NET Core MVC, Razor Views, Entity Framework Core y SQL Server. Incluye catálogos, clientes, proveedores, inventario por depósito y órdenes de compra con recepción de mercadería.

> Estado del proyecto: desarrollo activo. Antes de utilizarlo en producción deben completarse las tareas de seguridad, concurrencia, despliegue y aceptación documentadas en `docs/ERP-V2-AUDITORIA-TECNICA.md` (si se distribuye ese informe junto al proyecto).

## Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core MVC / .NET 10 |
| Lenguaje | C# |
| ORM | Entity Framework Core 10 |
| Base de datos | SQL Server (LocalDB para desarrollo local) |
| Autenticación | ASP.NET Core Identity |
| Autorización | Roles `Administrador` y `Operador` |
| Frontend | Razor Views, Bootstrap 5, JavaScript |
| Pruebas | xUnit v3, EF Core SQLite en memoria |

## Módulos actuales

- Administración de usuarios y roles.
- Catálogos: categorías, artículos y unidades de medida.
- Inventario: depósitos, stock, movimientos y transferencias.
- Compras: proveedores, órdenes de compra, líneas y recepción.
- Clientes.
- Paneles de inicio según permisos.

## Requisitos de desarrollo

- SDK de .NET 10.
- Visual Studio 2022 actualizado o editor compatible.
- SQL Server LocalDB (incluido en las instalaciones habituales de Visual Studio en Windows).
- Git.

## Configuración local

1. Clonar el repositorio y abrir `TransformadoresApp.sln`.
2. Restaurar paquetes:

   ```powershell
   dotnet restore TransformadoresApp.sln
   ```

3. Configurar la conexión en `TransformadoresApp/appsettings.json` o, preferentemente, mediante la variable de entorno `ConnectionStrings__DefaultConnection`.

   Valor de desarrollo LocalDB:

   ```text
   Server=(localdb)\MSSQLLocalDB;Database=TransformadoresDb;Trusted_Connection=True;MultipleActiveResultSets=true
   ```

4. Configurar las credenciales del administrador inicial mediante variables de entorno **antes del primer arranque**. No hay una contraseña predeterminada en el código:

   ```powershell
   $env:BootstrapAdmin__Email = "admin@tu-dominio.local"
   $env:BootstrapAdmin__Password = "Usa-Una-Frase-Larga-2026!"
   ```

   La contraseña debe cumplir la política de Identity (12 caracteres como mínimo, mayúscula, minúscula, número y carácter no alfanumérico). Elegí una contraseña única y no uses el ejemplo. Si tu LocalDB ya fue inicializada con la versión anterior, la cuenta antigua no se elimina ni cambia automáticamente: actualizá su contraseña y revisá las cuentas existentes antes de exponer la aplicación.

5. Ejecutar:

   ```powershell
   dotnet run --project TransformadoresApp
   ```

   En Development, la aplicación aplica las migraciones y carga las unidades de medida iniciales. El administrador se crea si se configuraron ambas variables `BootstrapAdmin__Email` y `BootstrapAdmin__Password` y ese email aún no existe.

6. Después de confirmar que el administrador inicial fue creado, eliminá las variables de entorno del proceso/sesión. La cuenta y su rol permanecen en la base de datos.

## Pruebas

Desde la raíz de la solución:

```powershell
dotnet test TransformadoresApp.sln
```

La suite actual cubre principalmente reglas de servicios de inventario y compras. Se está ampliando para incluir controladores, Identity, seguridad, concurrencia y flujos completos.

## Dependencias frontend

Las dependencias estáticas utilizadas por las vistas se encuentran en `TransformadoresApp/wwwroot/lib`. Esa carpeta debe conservarse/versionarse o restaurarse mediante un manifiesto reproducible; no debe excluirse de una distribución limpia, ya que las vistas referencian sus rutas directamente.

## Estructura

```text
TransformadoresApp/
├── Areas/Identity/       # Páginas de autenticación
├── Controllers/          # Endpoints MVC
├── Data/                # DbContext, seeders y configuración de datos
├── Migrations/           # Migraciones EF Core
├── Models/              # Entidades de dominio
├── Services/             # Servicios de Inventario y Compras
├── Views/                # Vistas y formularios Razor
└── wwwroot/               # CSS, JavaScript, imágenes y librerías
TransformadoresApp.Tests/  # Pruebas automatizadas
TransformadoresApp.sln
```

## Seguridad y despliegue

- El registro público está deshabilitado; las cuentas deben provisionarse por administración.
- No almacenar contraseñas, claves ni connection strings sensibles en el repositorio.
- LocalDB es solo para desarrollo local. Para producción, configurar una instancia de SQL Server adecuada y permisos mínimos.
- En producción, las migraciones no se ejecutan automáticamente al iniciar la web. Aplicarlas desde el proceso de despliegue (por ejemplo, con `dotnet ef database update --project TransformadoresApp` después de instalar `dotnet-ef` 10.x) y verificar el resultado en staging.
- Antes de desplegar, configurar secretos, backups y restauración, y ejecutar build/tests y pruebas de aceptación.
- La aplicación aún requiere una revisión final de concurrencia de stock/recepciones y pruebas de integración antes de considerarse apta para comercialización.

## Autor

Desarrollado por Gastón Galotto.
