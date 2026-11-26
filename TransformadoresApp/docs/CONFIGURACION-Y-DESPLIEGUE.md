# Configuración y despliegue — ERP V2

## Configuración sensible

No guardar secretos en `appsettings.json` ni en Git. La configuración de ASP.NET Core admite variables de entorno con doble guion bajo:

| Clave | Variable de entorno |
|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `BootstrapAdmin:Email` | `BootstrapAdmin__Email` |
| `BootstrapAdmin:Password` | `BootstrapAdmin__Password` |

El bootstrap crea el administrador configurado solo si ese email aún no existe. Una vez creado, quitar las variables de bootstrap del entorno de ejecución. Si se reutiliza una base anterior a este cambio, revisar y cambiar la contraseña del administrador histórico antes de publicar.

## Preparación de base de datos

- Desarrollo: con `ASPNETCORE_ENVIRONMENT=Development`, el arranque ejecuta las migraciones automáticamente.
- Staging/producción: aplicar migraciones como tarea de release antes de iniciar la aplicación. No conceder permisos DDL permanentes a la identidad de la aplicación web.
- Herramienta EF: instalar `dotnet-ef` de la misma línea mayor que EF Core (10.x) en el entorno de despliegue.

Comando orientativo desde la raíz de la solución:

```powershell
dotnet ef database update --project TransformadoresApp --startup-project TransformadoresApp
```

Revisar primero la migración y probarla sobre una copia de la base. Para una actualización real, realizar backup y tener procedimiento de rollback/restore.

## Publicación

```powershell
dotnet restore TransformadoresApp.sln
dotnet build TransformadoresApp.sln -c Release
dotnet test TransformadoresApp.sln -c Release
dotnet publish TransformadoresApp/TransformadoresApp.csproj -c Release -o publish
```

Configurar `ASPNETCORE_ENVIRONMENT=Production`, conexión de producción, HTTPS y secretos en el proveedor de hosting. LocalDB es exclusivamente para desarrollo Windows.

## Checklist previo a producción

- [ ] Build y tests completados en el SDK .NET 10 objetivo.
- [ ] Base de datos migrada y revisada en staging.
- [ ] Cuenta inicial segura y sin credenciales predeterminadas.
- [ ] Registro público deshabilitado; altas privadas desde Administración.
- [ ] Roles y antiforgery probados con usuarios no administradores.
- [ ] Backups automáticos y restauración ensayada.
- [ ] Logs sin contraseñas, tokens ni datos personales innecesarios.
- [ ] HTTPS, certificado, proxy y cookies verificados en hosting.
- [ ] Pruebas de concurrencia de stock y recepciones completadas.
- [ ] Pruebas de aceptación de compras, recepción, transferencias y ajustes.
- [ ] Política de soporte, actualizaciones y licencia definida.
