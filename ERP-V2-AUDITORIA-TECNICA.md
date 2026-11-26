# ERP V2 — Auditoría técnica inicial (código fuente del ZIP)

**Base revisada:** `ERP-V2.zip` recibido el 25/11/2026.  
**Alcance estático:** solución, proyecto web, modelos, controladores, servicios, Identity, configuración, migraciones, vistas, assets y proyecto de tests.

## Alcance y límites

- Se listaron 275 archivos del proyecto web, 15 controladores, 13 archivos de servicios, 17 archivos de modelos y 8 archivos C# de pruebas.
- Se detectaron 121 atributos `[Fact]`/`[Theory]` (incluye una prueba nueva de validación de teléfono de proveedor) mediante búsqueda estática; la mayoría de los tests existentes se concentran en servicios de Inventario y Compras.
- **No se pudo compilar ni ejecutar `dotnet test` en este entorno:** el ejecutable `dotnet` no está instalado/disponible. Por eso este documento distingue los hallazgos estáticos de las verificaciones que aún requieren ejecución.
- Esta auditoría no certifica cumplimiento legal, seguridad de infraestructura, rendimiento bajo carga ni aceptación funcional de usuarios finales.

## Resumen ejecutivo

El proyecto tiene una base funcional valiosa: MVC + EF Core, Identity y roles, entidades de negocio, servicios para inventario y compras, transacciones en transferencias/recepciones, índices únicos importantes y una suite de tests considerable para los servicios existentes.

Sin embargo, **la copia auditada no debe considerarse lista para producción o comercialización todavía**. Hay riesgos de seguridad en el bootstrap de usuarios, registro de Identity y protección CSRF; además, las operaciones críticas de stock/recepción necesitan protección contra concurrencia. La documentación y el proceso de instalación no representan el estado actual del código.

## Hallazgos priorizados

### P0 — Bloqueantes antes de publicar

1. **Credencial de administrador fija en `Data/IdentitySeeder.cs`.** Se intenta crear `admin@rostagno.com` con una contraseña literal conocida en el código. Una credencial conocida en el código no puede utilizarse en una instalación comercial. Debe reemplazarse por un bootstrap seguro basado en secreto de entorno/User Secrets o una operación de provisión de administrador de un solo uso; no debe quedar un password predeterminado.
2. **La página de registro estándar de Identity sigue implementada.** `Program.cs` registra `.AddDefaultUI()` y `app.MapRazorPages()`. `Areas/Identity/Pages/Account/Register.cshtml.cs` contiene el flujo real que crea usuarios y los inicia sesión. Esto contradice el mensaje del `AuthController.Register` que indica que el registro público está deshabilitado. Debe deshabilitarse la página estándar o configurarse un flujo de alta solo por administrador.
3. **POST de administración de roles sin protección antifalsificación explícita.** `AdminController.AddToRole` y `RemoveFromRole` tienen `[HttpPost]` pero no `[ValidateAntiForgeryToken]`. Las vistas contienen tokens, pero las acciones deben quedar protegidas explícitamente y/o por filtro global MVC.

### P1 — Alta prioridad funcional y de integridad

4. **Política de contraseña demasiado permisiva.** En `Program.cs`: longitud mínima 6, sin dígitos, mayúsculas, minúsculas ni caracteres no alfanuméricos requeridos. Debe elevarse y documentarse, considerando el procedimiento para usuarios existentes.
5. **Migraciones y seed ejecutados automáticamente al arrancar.** `Program.cs` llama `Database.Migrate()`, `DbSeeder.Seed()` e `IdentitySeeder.SeedRolesAndAdminAsync()` en cada inicio. Esto acopla el arranque web a permisos DDL y puede provocar carreras o indisponibilidad en despliegues con varias instancias. Debe definirse un proceso de migración/seed separado para producción.
6. **Recepciones concurrentes no protegidas por control de concurrencia.** `PurchaseReceivingService.ReceiveAsync` usa transacción, pero la lectura y actualización de `ReceivedQuantity` no usa token de concurrencia ni bloqueo/actualización atómica. Dos recepciones simultáneas pueden validar contra la misma cantidad pendiente.
7. **Operaciones de stock vulnerables a actualizaciones concurrentes.** `StockService` y `StockTransferService` validan cantidades leídas y luego actualizan entidades. La transacción de transferencia es buena, pero no evita por sí sola que dos operaciones concurrentes descuenten sobre la misma lectura. Se necesita estrategia de concurrencia en base de datos y pruebas paralelas.
8. **Índice no único para líneas de compra.** `ApplicationDbContext` crea índice sobre `(PurchaseOrderId, ItemId)` sin `IsUnique()`. El servicio chequea duplicados en aplicación, pero dos solicitudes concurrentes pueden insertar líneas duplicadas. Si el negocio requiere una línea por artículo y orden, el índice debe ser único y acompañado de migración y manejo de conflicto.
9. **Ruta de unobtrusive validation rota en el ZIP auditado.** `Views/Shared/_ValidationScriptsPartial.cshtml` apunta a `.../dist/jquery.validate.unobtrusive.js/jquery.validate.unobtrusive.min.js`, pero el archivo físico está en `wwwroot/lib/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js`. El chequeo de referencias estáticas detectó exactamente esa referencia faltante.
10. **Validaciones de importes/cantidades de catálogo incompletas en el modelo.** `Models/Catalogs/Item.cs` declara `Cost`, `Price` y `MinimumStock` como `decimal`, pero no tiene rangos que impidan valores negativos. El controlador `ItemsController.Create/Edit` tampoco aplica explícitamente validaciones de negocio para esos importes antes de guardar. Deben definirse reglas de dominio y restricciones de base de datos.
11. **Validación de proveedor activo al crear/editar órdenes.** `PurchaseOrderService.CreateAsync/UpdateAsync` comprueba número y estado de orden, pero no verifica explícitamente que el proveedor exista y esté activo antes de aceptar la orden. La FK evita referencias inexistentes, no proveedores desactivados.
12. **Model binding directo a entidades en varias acciones.** Varias acciones reciben entidades EF directamente (`Item`, `Category`, `PurchaseOrder`, `PurchaseOrderItem`, etc.). Algunas propiedades se reasignan cuidadosamente, otras pueden aceptar campos que no deberían ser controlados desde el formulario. Debe adoptarse binding explícito/DTOs o listas `[Bind]` y probar overposting por acción.
13. **Resultados de Identity ignorados en administración.** `AddToRole` y `RemoveFromRole` no comprueban `IdentityResult`; puede mostrarse éxito aunque la operación falle. También debe protegerse el último administrador, no solo una cuenta identificada por email fijo.
14. **Patrones HTML inválidos en formularios de clientes/proveedores.** Se detectaron atributos `pattern` que duplicaban reglas de DataAnnotations. Se verificaron con JavaScript moderno usando el modo Unicode Sets (`v`): los patrones de proveedores fallan por guiones/clases de caracteres no válidos. La solución debe tener una sola fuente de verdad y pruebas browser-side.

### P2 — Calidad, mantenimiento y producto

15. **README desactualizado.** Declara ASP.NET Core/.NET 8, describe transformadores y órdenes de producción que ya no corresponden al código actual y documenta rutas de carpetas que no coinciden. El proyecto apunta a `net10.0`.
16. **Dependencias frontend excluidas por `.gitignore`.** El archivo ignora `wwwroot/lib/`. Aunque las librerías están presentes en el ZIP, el repositorio debe asegurar que un clone limpio obtiene jQuery, Bootstrap, jQuery Validation, unobtrusive, IMask e intl-tel-input mediante archivos versionados o un manifiesto de restauración reproducible.
17. **No hay pruebas de integración MVC/Identity/HTTP.** Los tests actuales se enfocan en servicios de inventario y compras. Faltan tests para controladores, autorización, CSRF, Customer/Supplier/CRUD, registro/login y migraciones.
18. **Paginación de usuarios no escalable.** `AdminController.UserManagement` materializa todos los usuarios en memoria antes de paginar (`ToList` y luego `Skip/Take`), además de consultar roles uno por uno. Debe paginar desde SQL y optimizar la carga de roles.
19. **Protecciones de ciclo de vida de entidades.** `CategoriesController.Edit` solo contempla la autorreferencia dentro de una rama condicional de `ItemType` y no previene ciclos de varios niveles. Deben revisarse categorías jerárquicas, desactivación de artículos/depósitos con stock y reactivación de órdenes, para evitar estados operativos inválidos.
20. **Trazabilidad insuficiente para ERP comercial.** `StockMovement` registra artículo, depósito, tipo, cantidad, nota y fecha, pero no registra usuario, referencia de origen (orden/recepción/transferencia) ni identificador de operación para idempotencia. Se recomienda un historial auditable y reconciliación stock vs. movimientos.
21. **Despliegue no documentado.** La cadena de conexión usa SQL Server LocalDB, adecuado para desarrollo Windows, no para un servidor de producción. Faltan instrucciones/configuración de producción, manejo de secretos, migración de base de datos, backup/restore, health checks y CI de build/test.
22. **Artefactos locales en solución.** El ZIP contiene `TransformadoresApp.csproj.user`; debe excluirse de entregables y repositorio.

## Aspectos positivos observados

- Autorización por roles aplicada a nivel de clase en los controladores de módulos.
- La mayoría de los POST de módulos incluye `[ValidateAntiForgeryToken]`.
- Índices únicos para códigos de artículos, depósitos, clientes, proveedores y número de orden.
- Restricciones de borrado para relaciones importantes y soft delete en varios catálogos.
- Transferencias y recepciones usan transacciones y rollback.
- Suite de pruebas existente amplia para reglas de servicios de inventario y compras.

## Plan recomendado de trabajo

### Fase 1 — Seguridad y arranque seguro
- Eliminar password fijo de administrador y crear bootstrap configurable de una sola vez.
- Deshabilitar el registro público de Identity de forma efectiva.
- Endurecer política de contraseñas, lockout y cookies.
- Aplicar antiforgery global a MVC y explícito en acciones administrativas.
- Separar migración/seed del arranque normal de producción.
- Revisar y actualizar la ruta de unobtrusive validation y las referencias estáticas.

### Fase 2 — Integridad de datos y reglas de negocio
- Añadir reglas positivas para cantidades/precios y restricciones DB.
- Añadir índice único de líneas de compra (si se confirma regla de una línea por artículo/OC) y migración.
- Validar proveedor/artículo/depósito activo y estados permitidos.
- Añadir concurrencia optimista/atómica a stock y recepciones.
- Definir idempotencia y referencias de movimientos.
- Prevenir ciclos de categoría y estados incompatibles de soft delete.

### Fase 3 — Arquitectura y mantenibilidad
- Reemplazar binding directo de entidades por ViewModels/DTOs en flujos críticos.
- Unificar validaciones y respuestas de error; no perder ModelState en redirects.
- Centralizar manejo de errores y logging estructurado.
- Revisar paginación, consultas, índices y N+1.
- Revisar accesibilidad, responsive y consistencia de UX en todos los módulos.

### Fase 4 — Calidad y comercialización
- Tests unitarios faltantes, tests de integración y tests de autorización/CSRF.
- CI automatizado: restore, build, test, análisis estático y publicación.
- README nuevo, guía de instalación, variables de entorno, administrador inicial, backup/restore y despliegue.
- Licencia, versión, changelog, capturas actualizadas, demo con datos ficticios y portfolio.
- Pruebas de aceptación con flujos completos de compras e inventario.

## Criterio de salida

No declarar “listo para producción” hasta que:
1. restore/build/test pasen en SDK .NET 10;
2. migraciones se apliquen sobre una base limpia y una base de actualización;
3. registro y bootstrap de administrador sean seguros;
4. autorización y CSRF se prueben por HTTP;
5. stock/recepciones pasen pruebas de concurrencia y reconciliación;
6. flujos completos se prueben manualmente en navegador;
7. despliegue, secretos, backups y recuperación estén documentados.

## Cambios de primera fase preparados en esta copia de trabajo

Además del diagnóstico anterior, en la copia de trabajo se prepararon estas correcciones de seguridad y mantenimiento:

- La política de contraseña ahora exige 12 caracteres, mayúscula, minúscula, dígito y símbolo; se configuró bloqueo tras 5 intentos durante 15 minutos y email único.
- El administrador inicial ya no usa una contraseña hardcodeada: se provisiona mediante `BootstrapAdmin:Email` y `BootstrapAdmin:Password` (variables de entorno `BootstrapAdmin__Email` y `BootstrapAdmin__Password`).
- La página estándar de registro de Identity se reemplazó por una página informativa deshabilitada.
- Se agregó antiforgery global a MVC y explícito en las acciones de roles de Administración.
- La administración de roles ahora valida roles permitidos, comprueba resultados de Identity y evita quitar el rol al último administrador o al administrador autenticado. Se agregó creación de usuarios desde Administración porque al deshabilitar el registro público era necesario conservar un flujo privado de alta.
- La lista de usuarios ahora pagina desde la consulta SQL en vez de cargar todos los usuarios antes de paginar.
- Las migraciones automáticas se limitaron a Development; producción debe recibir la base migrada desde el proceso de despliegue.
- Se corrigió la ruta de unobtrusive validation; se quitaron patrones HTML duplicados de Clientes y Proveedores, se trasladó la validación del teléfono de proveedor al modelo; se dejó de ignorar `wwwroot/lib/` en Git y se actualizó el README a .NET 10 y los módulos actuales.
- Se retiró el archivo local `TransformadoresApp.csproj.user` del paquete de trabajo. Se configuraron cookies seguras de sesión y se habilitó lockout en la página estándar de login de Identity. Se agregó una guía de configuración/despliegue y una prueba de validación del teléfono del proveedor.

Estos cambios están preparados en el paquete de primera fase, pero **no se declaran verificados por compilación** porque este entorno no dispone de `dotnet`. Deben ejecutarse `dotnet restore`, `dotnet build` y `dotnet test` en una máquina con SDK .NET 10 antes de integrarlos en la rama principal.
