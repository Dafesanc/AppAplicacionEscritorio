# 🚀 GUÍA RÁPIDA DE IMPLEMENTACIÓN - BASE DE DATOS

## ⚡ Pasos para Implementar la BD

### 1️⃣ Preparar SQL Server Express
```
✓ Descargar SQL Server Express 2019 o 2022 (GRATIS)
✓ Instalar con: SQL Server Management Studio (SSMS)
✓ Descargar SSMS (gratis también)
✓ Validar que SQL Server esté ejecutándose
```

**Descargas:**
- SQL Server Express: https://www.microsoft.com/es-es/sql-server/sql-server-editions-express
- SQL Server Management Studio: https://learn.microsoft.com/es-es/sql/ssms/download-sql-server-management-studio-ssms

---

### 2️⃣ Crear la Base de Datos

**Opción A: Ejecutar Script SQL (RECOMENDADO)**
```
1. Abrir SQL Server Management Studio
2. Conectarse al servidor local
3. Crear nueva consulta (New Query)
4. Copiar y pegar el contenido de: SCRIPT_BASE_DATOS.sql
5. Ejecutar con F5 o botón Run
6. Esperar confirmación: ✓ Base de datos creada exitosamente
```

**Opción B: Script por Lines**
```sql
-- Ejecutar línea por línea en SSMS
-- Comenzar desde: USE master;
-- Hasta el final del archivo
```

---

### 3️⃣ Validar Creación

**Verificar tablas creadas:**
```sql
USE GestionComercial_BD;
GO

-- Ver todas las tablas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' 
ORDER BY TABLE_NAME;
GO

-- Resultado esperado: 13 tablas
```

**Verificar datos iniciales:**
```sql
-- Verificar usuarios
SELECT * FROM Usuarios;

-- Verificar clientes
SELECT * FROM Clientes;

-- Verificar vehículos
SELECT * FROM Vehiculos;

-- Verificar productos
SELECT * FROM Productos;
```

---

### 4️⃣ Conectar desde Aplicación .NET

**String de Conexión (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GestionComercial_BD;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Alternativas de String de Conexión:**
```
// Local machine (nombre del equipo)
Server=(local);Database=GestionComercial_BD;Trusted_Connection=true;

// Instancia específica
Server=.\\SQLEXPRESS;Database=GestionComercial_BD;Trusted_Connection=true;

// Con credenciales SQL
Server=localhost;Database=GestionComercial_BD;User Id=sa;Password=TuContraseña;
```

**En C# (Entity Framework Core):**
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(
        @"Server=.\SQLEXPRESS;Database=GestionComercial_BD;Trusted_Connection=true;"
    );
}
```

---

### 5️⃣ Verificar Conectividad desde Código

**Test en C#:**
```csharp
using (var context = new GestionComercialContext())
{
    try
    {
        context.Database.OpenConnection();
        Console.WriteLine("✓ Conexión exitosa");
        context.Database.CloseConnection();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error: {ex.Message}");
    }
}
```

---

## 📊 Estadísticas de BD

| Concepto | Cantidad |
|----------|----------|
| Tablas | 13 |
| Índices | 15+ |
| Vistas | 4 |
| Triggers | 1 |
| Relaciones Foreign Key | 20+ |
| Usuarios Iniciales | 2 (admin, operador) |
| Clientes de Ejemplo | 2 |
| Vehículos de Ejemplo | 3 |
| Productos de Ejemplo | 4 |

---

## 🔐 Credenciales Iniciales

### Usuario Administrador
```
Usuario: admin
Contraseña: 123456  ⚠️ CAMBIAR INMEDIATAMENTE
Email: admin@gestioncomercial.com
Rol: Administrador
Estado: ACTIVO
```

**⚠️ IMPORTANTE:**
- La contraseña está en MD5 (hash: e99a18c428cb38d5f260853678922e03)
- DEBE ser cambiada en la primera ejecución
- Usar bcrypt o SHA256 en el hash real

### Usuario Operador
```
Usuario: operador
Contraseña: 123456  ⚠️ CAMBIAR INMEDIATAMENTE
Email: operador@gestioncomercial.com
Rol: Operador
Estado: ACTIVO
```

---

## 🛠️ Mantenimiento de BD

### Backup Automático
```sql
-- Crear backup manual (recomendado antes de cambios)
BACKUP DATABASE [GestionComercial_BD] 
TO DISK = 'C:\Backups\GestionComercial_BD_2026-04-17.bak';
GO

-- Restaurar desde backup
RESTORE DATABASE [GestionComercial_BD] 
FROM DISK = 'C:\Backups\GestionComercial_BD_2026-04-17.bak' 
WITH REPLACE;
GO
```

### Limpiar Datos de Prueba
```sql
-- Eliminar toda la auditoría
DELETE FROM Auditoria;

-- Eliminar todas las ventas (cascada elimina detalles)
DELETE FROM Ventas;

-- Eliminar facturas
DELETE FROM Facturas;

-- Reiniciar identidades
DBCC CHECKIDENT('Clientes', RESEED, 0);
DBCC CHECKIDENT('Vehiculos', RESEED, 0);
DBCC CHECKIDENT('Productos', RESEED, 0);
```

### Consultas Útiles

**Top 5 clientes por monto:**
```sql
SELECT TOP 5 
    c.Nombre,
    COUNT(v.ID_Venta) AS TotalVentas,
    SUM(v.TotalVenta) AS MontoTotal,
    AVG(v.TotalVenta) AS PromedioVenta
FROM Clientes c
LEFT JOIN Ventas v ON c.ID_Cliente = v.ID_Cliente
GROUP BY c.ID_Cliente, c.Nombre
ORDER BY MontoTotal DESC;
```

**Productos con stock bajo:**
```sql
SELECT 
    Codigo,
    Nombre,
    Stock,
    StockMinimo,
    (StockMinimo - Stock) AS UnidadesRequeridas
FROM Productos
WHERE Stock < StockMinimo
ORDER BY UnidadesRequeridas DESC;
```

**Auditoría de cambios recientes:**
```sql
SELECT TOP 20
    u.NombreCompleto,
    a.Tabla,
    a.TipoOperacion,
    a.FechaOperacion,
    a.Razon
FROM Auditoria a
INNER JOIN Usuarios u ON a.ID_Usuario = u.ID_Usuario
ORDER BY a.FechaOperacion DESC;
```

**Vehículos sin pesajes en los últimos 7 días:**
```sql
SELECT 
    v.Placa,
    c.Nombre AS Cliente,
    v.UltimaPesada,
    DATEDIFF(DAY, v.UltimaPesada, GETDATE()) AS DiasSinPesaje
FROM Vehiculos v
INNER JOIN Clientes c ON v.ID_Cliente = c.ID_Cliente
WHERE v.UltimaPesada IS NULL 
   OR DATEDIFF(DAY, v.UltimaPesada, GETDATE()) > 7;
```

---

## ❌ Troubleshooting

### Error: "Cannot connect to SQL Server"
**Solución:**
```
1. Verificar que SQL Server esté ejecutándose:
   - Services → SQL Server (SQLEXPRESS)
   
2. Verificar conexión local:
   - sqlcmd -S (local) -E
   
3. Habilitar TCP/IP:
   - SQL Server Configuration Manager
   - Protocols for SQLEXPRESS
   - Habilitar TCP/IP
```

### Error: "Login failed for user '(Windows)'"
**Solución:**
```
1. Usar Trusted Connection = true (Windows Auth)
2. O crear login SQL con usuario/contraseña
3. Asegurar permisos en la BD
```

### Error: "Database already exists"
**Solución:**
```sql
-- Eliminar BD existente
DROP DATABASE [GestionComercial_BD];

-- Luego ejecutar script nuevamente
```

---

## 📈 Próximos Pasos

1. **✓ BD creada** (COMPLETADO)
2. ⏳ Crear modelos Entity Framework (DbContext, DbSet)
3. ⏳ Implementar repositorio pattern
4. ⏳ Crear servicios de negocio
5. ⏳ Implementar integración RS232 (báscula)
6. ⏳ Crear UI en WPF
7. ⏳ Implementar reportes
8. ⏳ Crear auditoría automatizada

---

## 📚 Archivos Generados

```
c:\Users\super\Documents\Proyectos full stack\Aplicaciones Escritorio\
├── ANALISIS_DETALLADO_2.0_4.0.md      ← Análisis funcional
├── SCRIPT_BASE_DATOS.sql               ← Script de creación (ESTE ARCHIVO)
├── ESTRUCTURA_BASE_DATOS.md            ← ER diagram y relaciones
└── GUIA_RAPIDA_IMPLEMENTACION.md       ← Este archivo
```

---

## ✅ Checklist de Validación

- [ ] SQL Server instalado y ejecutándose
- [ ] SQL Server Management Studio instalado
- [ ] Script SQL ejecutado sin errores
- [ ] 13 tablas creadas
- [ ] Datos iniciales insertados
- [ ] Connection string configurada
- [ ] Conectividad verificada desde código
- [ ] Credenciales iniciales cambiadas
- [ ] Backup inicial realizado
- [ ] Documentación completada

---

**¿Listo para comenzar?** 🚀

Una vez completados estos pasos, estamos listos para:
1. Crear la arquitectura Clean Architecture en .NET
2. Implementar Entity Framework
3. Dar vida a la aplicación WPF
