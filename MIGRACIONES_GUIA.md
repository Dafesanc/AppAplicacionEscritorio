# Guía de Migraciones de Base de Datos

## Prerequisitos

- .NET 8.0 SDK instalado
- SQL Server Express instalado y ejecutándose
- (Opcional) SQL Server Management Studio (SSMS) para verificación manual

---

## 1. Configuración Inicial

### Verificar Cadena de Conexión

En `GestionComercial.Presentation/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local)\\SQLEXPRESS;Database=GestionComercial_BD;Trusted_Connection=true;Encrypt=false;"
  }
}
```

**Valores comunes:**
- `(local)\SQLEXPRESS` - SQL Server Express local
- `localhost\SQLEXPRESS` - Alternativa
- `./` - SQLite (alternativa para desarrollo)

---

## 2. Generar Migraciones

Desde la terminal en la raíz del proyecto:

### Opción A: Crear nueva migración (después de cambios en entities)

```bash
# Navegar a la carpeta del proyecto
cd c:\Users\super\Documents\Proyectos\ full\ stack\Aplicaciones\ Escritorio

# Crear migración inicial
dotnet ef migrations add InitialCreate --project GestionComercial.Infrastructure

# Crear migración después de cambios
dotnet ef migrations add "NombreMigracion" --project GestionComercial.Infrastructure
```

### Opción B: Ver migraciones pendientes

```bash
dotnet ef migrations list --project GestionComercial.Infrastructure
```

---

## 3. Aplicar Migraciones (Crear la Base de Datos)

### Opción A: Comando EF Core (Recomendado)

```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update --project GestionComercial.Infrastructure

# Aplicar hasta una migración específica
dotnet ef database update InitialCreate --project GestionComercial.Infrastructure
```

### Opción B: Script SQL Directo (Alternativa)

Si prefieres usar SQL Server Management Studio:

1. Abre `SCRIPT_BASE_DATOS.sql`
2. Conéctate a SQL Server Express en SSMS
3. Ejecuta el script completo
4. La base de datos y todos los datos iniciales se crearán

---

## 4. Verificar Migración Exitosa

### En SSMS (SQL Server Management Studio)

```sql
-- Verificar que la BD existe
SELECT name FROM sys.databases WHERE name = 'GestionComercial_BD';

-- Ver todas las tablas creadas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' 
ORDER BY TABLE_NAME;

-- Verificar datos iniciales
SELECT * FROM Usuarios;        -- Debe tener admin y operador
SELECT * FROM Clientes;        -- Debe tener 2 clientes de prueba
SELECT * FROM Productos;       -- Debe tener 4 productos
SELECT * FROM Vehiculos;       -- Debe tener 3 vehículos
```

### Desde la Aplicación

```csharp
// En Program.cs o startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GestionComercialContext>();
    
    // Aplicar migraciones pendientes
    context.Database.Migrate();
    
    // Verificar conexión
    bool canConnect = context.Database.CanConnect();
    Console.WriteLine($"Conexión a BD: {(canConnect ? "✓ Exitosa" : "✗ Fallida")}");
}
```

---

## 5. Problemas Comunes y Soluciones

### Error: "The database does not exist"

**Solución 1:** Ejecutar migración
```bash
dotnet ef database update --project GestionComercial.Infrastructure
```

**Solución 2:** Crear BD manualmente primero
```sql
-- En SSMS
CREATE DATABASE GestionComercial_BD;
```

### Error: "No parameterless constructor found"

Asegúrate que `GestionComercialContext` tiene constructor sin parámetros O usa `AddDbContext` en DI.

### Error: "Migration 'X' has already been applied"

```bash
# Ver migración actual aplicada
dotnet ef migrations list --project GestionComercial.Infrastructure

# Revertir una migración
dotnet ef database update NombreMigracionAnterior --project GestionComercial.Infrastructure
```

### Error: "Cadena de conexión no encontrada"

Verificar en `appsettings.json` que `DefaultConnection` existe y está bien escrita.

---

## 6. Reverter Migraciones (Si es Necesario)

```bash
# Revertir la última migración (borra la BD)
dotnet ef database update 0 --project GestionComercial.Infrastructure

# Revertir a una migración específica
dotnet ef database update InitialCreate --project GestionComercial.Infrastructure

# Eliminar una migración pendiente (no aplicada)
dotnet ef migrations remove --project GestionComercial.Infrastructure
```

---

## 7. Workflow Completo de Desarrollo

### Cuando crees nuevas Entities:

1. **Crear Entity** en `GestionComercial.Domain/Entities/`
2. **Configurar** en `GestionComercialContext.OnModelCreating()`
3. **Generar Migración**
   ```bash
   dotnet ef migrations add "DescripcionCambio" --project GestionComercial.Infrastructure
   ```
4. **Revisar** archivo generado en `Migrations/`
5. **Aplicar Migración**
   ```bash
   dotnet ef database update --project GestionComercial.Infrastructure
   ```

### Cuando modifiques Entities:

```bash
# El proceso es igual: cambio → migración → update
dotnet ef migrations add "DescripcionCambio" --project GestionComercial.Infrastructure
dotnet ef database update --project GestionComercial.Infrastructure
```

---

## 8. Variables de Entorno / Configuración Alternativa

### Para SQLite (Desarrollo Local)

En `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=GestionComercial.db"
  }
}
```

En `DependencyInjection.cs`:
```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Para SQLite:
    services.AddDbContext<GestionComercialContext>(options =>
        options.UseSqlite(connectionString));
    
    // Para SQL Server:
    // options.UseSqlServer(connectionString);
}
```

---

## 9. Scripts Útiles

### Script: Crear BD + Aplicar Migraciones (PowerShell)

```powershell
# run-migrations.ps1

$projectPath = "c:\Users\super\Documents\Proyectos full stack\Aplicaciones Escritorio"
Set-Location $projectPath

Write-Host "Limpiando migraciones previas..." -ForegroundColor Yellow
dotnet ef database update 0 --project GestionComercial.Infrastructure

Write-Host "Aplicando migraciones..." -ForegroundColor Green
dotnet ef database update --project GestionComercial.Infrastructure

Write-Host "✓ Migraciones aplicadas exitosamente" -ForegroundColor Green
```

### Script: Verificar Integridad BD (SQL)

```sql
-- Verificar todas las tablas
SELECT 
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS c WHERE c.TABLE_NAME = t.TABLE_NAME) AS NumeroColumnas,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE k WHERE k.TABLE_NAME = t.TABLE_NAME) AS NumeroForeignKeys
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;
```

---

## 10. Backup y Restore

### Crear Backup

```sql
BACKUP DATABASE GestionComercial_BD
TO DISK = 'C:\Backups\GestionComercial_BD.bak'
WITH FORMAT;
```

### Restaurar desde Backup

```sql
RESTORE DATABASE GestionComercial_BD
FROM DISK = 'C:\Backups\GestionComercial_BD.bak'
WITH REPLACE;
```

---

## 11. Comandos Rápidos de Referencia

| Comando | Descripción |
|---------|------------|
| `dotnet ef` | Ver ayuda general |
| `dotnet ef migrations add Nombre` | Crear nueva migración |
| `dotnet ef migrations list` | Ver todas las migraciones |
| `dotnet ef database update` | Aplicar migraciones |
| `dotnet ef database update 0` | Revertir todas (borra BD) |
| `dotnet ef database drop` | Eliminar BD |
| `dotnet ef migrations remove` | Eliminar última migración no aplicada |

---

## Contacto / Soporte

Si tienes problemas:
1. Revisa el `appsettings.json` - Cadena de conexión
2. Verifica que SQL Server está ejecutándose
3. Revisa la salida de error detallada: `dotnet ef database update --verbose`
4. Revisa archivos en `Persistence/Migrations/`

---

**Última actualización:** 17 de Abril de 2026
