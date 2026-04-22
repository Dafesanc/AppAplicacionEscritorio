# 📊 ANÁLISIS EXHAUSTIVO - ESTRUCTURA COMPLETA DEL PROYECTO GESTIONCOMERCIAL

**Documento Generado:** 22-04-2026  
**Tipo de Proyecto:** Aplicación de Escritorio WPF (.NET 8.0)  
**Patrones:** Clean Architecture + MVVM + Dependency Injection

---

## 📑 TABLA DE CONTENIDOS

1. [Estructura General de Carpetas](#1-estructura-general-de-carpetas)
2. [Proyectos en la Solución](#2-proyectos-en-la-solución)
3. [ViewModels (Presentation Layer)](#3-viewmodels-presentation-layer)
4. [Views/XAML (Presentation Layer)](#4-viewsxaml-presentation-layer)
5. [Configuración de la Aplicación](#5-configuración-de-la-aplicación)
6. [Infraestructura y Persistencia](#6-infraestructura-y-persistencia)
7. [Capa de Aplicación](#7-capa-de-aplicación)
8. [Dependencias y NuGet Packages](#8-dependencias-y-nuget-packages)
9. [Flujo de Inicialización](#9-flujo-de-inicialización)
10. [Estado Actual y Próximos Pasos](#10-estado-actual-y-próximos-pasos)

---

## 1. ESTRUCTURA GENERAL DE CARPETAS

```
c:\Users\super\Documents\Proyectos full stack\Aplicaciones Escritorio\
│
├── GestionComercial.sln                          ← Archivo de solución
│
├── DOCUMENTACIÓN/
│   ├── ANALISIS_DETALLADO_2.0_4.0.md           ← Análisis extenso de objetivos
│   ├── ESTRUCTURA_BASE_DATOS.md                 ← Esquema ER
│   ├── ESTRUCTURA_PROYECTO_DOTNET.md            ← Documentación anterior
│   ├── GUIA_RAPIDA_IMPLEMENTACION.md            ← Setup guide
│   ├── MIGRACIONES_GUIA.md                      ← Instrucciones de migraciones
│   ├── RESUMEN_SERVICIOS_MIGRACIONES.md         ← Resumen de servicios
│   └── SCRIPT_BASE_DATOS.sql                    ← Script SQL inicial
│
├── GestionComercial.Domain/                      ← Capa de Dominio
│   ├── GestionComercial.Domain.csproj
│   └── Entities/
│       ├── Auditoria.cs                         ← Trazabilidad
│       ├── Cliente.cs                           ← Cliente principal
│       ├── DetalleVenta.cs                      ← Líneas de venta
│       ├── Factura.cs                           ← Comprobantes
│       ├── LoteInventario.cs                    ← Lotes de productos
│       ├── MovimientoInventario.cs              ← Auditoría de stock
│       ├── Pesaje.cs                            ← Lecturas báscula RS232
│       ├── Producto.cs                          ← Catálogo de materiales
│       ├── Rol.cs                               ← Roles del sistema
│       ├── Usuario.cs                           ← Autenticación
│       ├── Vehiculo.cs                          ← Vehículos de cliente
│       └── Venta.cs                             ← Transacciones
│
├── GestionComercial.Application/                 ← Capa de Aplicación
│   ├── GestionComercial.Application.csproj
│   ├── DTOs/
│   │   ├── ServiceDTOs.cs                       ← DTOs genéricos (futuro)
│   │   └── UsuarioDTO.cs                        ← DTOs de usuario
│   │       ├── UsuarioDTO                       ← Lectura
│   │       ├── CrearActualizarUsuarioDTO        ← Crear/actualizar
│   │       ├── LoginDTO                         ← Credenciales
│   │       └── LoginResponseDTO                 ← Respuesta login
│   │
│   ├── Interfaces/
│   │   ├── IClienteRepository.cs                ← Contrato Cliente
│   │   ├── IRepository.cs                       ← Genérico
│   │   ├── IUsuarioRepository.cs                ← Contrato Usuario
│   │   └── IVehiculoRepository.cs               ← Contrato Vehículo
│   │
│   └── Services/
│       ├── IServiceInterfaces.cs                ← Interfaces de servicios
│       ├── IUsuarioService.cs                   ← Contrato servicio usuario
│       ├── ClienteService.cs                    ← Negocio de clientes
│       ├── UsuarioService.cs                    ← Autenticación y usuarios
│       └── VentaService.cs                      ← Procesamiento de ventas
│
├── GestionComercial.Infrastructure/              ← Capa de Infraestructura
│   ├── GestionComercial.Infrastructure.csproj
│   ├── DependencyInjection.cs                   ← Registro de servicios
│   │
│   └── Persistence/
│       ├── GestionComercialContext.cs           ← DbContext (Entity Framework)
│       │
│       ├── Migrations/                          ← Migraciones de EF Core
│       │   └── [Archivos de migraciones]
│       │
│       └── Repositories/                        ← Implementaciones de repositorios
│           ├── Repository.cs                    ← Repositorio genérico
│           ├── AuditoriaRepository.cs
│           ├── ClienteRepository.cs
│           ├── FacturaRepository.cs
│           ├── LoteInventarioRepository.cs
│           ├── MovimientoInventarioRepository.cs
│           ├── PesajeRepository.cs
│           ├── ProductoRepository.cs
│           ├── UsuarioRepository.cs
│           ├── VehiculoRepository.cs
│           └── VentaRepository.cs
│
└── GestionComercial.Presentation/                ← Capa de Presentación (WPF)
    ├── GestionComercial.Presentation.csproj
    ├── appsettings.json                         ← Configuración de app
    ├── Program.cs                               ← Punto de entrada (comentario)
    ├── App.xaml                                 ← Configuración XAML
    ├── App.xaml.cs                              ← Inicialización de app
    │
    ├── Services/
    │   └── SessionService.cs                    ← Gestión de sesión usuario
    │
    ├── ViewModels/
    │   ├── LoginViewModel.cs                    ← MVVM para Login
    │   ├── ShellViewModel.cs                    ← MVVM para Shell principal
    │   │
    │   └── Modules/
    │       ├── ClientesViewModel.cs             ← Módulo de clientes
    │       ├── DashboardViewModel.cs            ← Módulo principal
    │       ├── FacturasViewModel.cs             ← Módulo de facturación
    │       ├── InventarioViewModel.cs           ← Módulo de inventario
    │       ├── PesajesViewModel.cs              ← Módulo de báscula
    │       ├── UsuariosViewModel.cs             ← Módulo de usuarios
    │       └── VentasViewModel.cs               ← Módulo de ventas
    │
    └── Views/
        ├── LoginWindow.xaml                     ← UI de login
        ├── LoginWindow.xaml.cs                  ← Code-behind login
        ├── MainShellWindow.xaml                 ← UI principal
        ├── MainShellWindow.xaml.cs              ← Code-behind principal
        │
        └── Modules/
            └── DashboardView.xaml               ← UI de Dashboard
│
└── GestionComercial.Tests/                       ← Capa de Tests
    ├── GestionComercial.Tests.csproj
    └── [Archivos de tests - por implementar]
```

---

## 2. PROYECTOS EN LA SOLUCIÓN

### A. `GestionComercial.Domain` (Capa de Dominio)
- **TargetFramework:** net8.0
- **Nullable:** enabled
- **Propósito:** Define las entidades de negocio puras
- **Contenido:** 12 entidades principales
- **Dependencias:** Microsoft.AspNetCore.Cryptography.KeyDerivation 8.0.0

**Entidades:**
| Entidad | Propósito | Relaciones |
|---------|----------|-----------|
| Rol | Definir permisos del sistema | 1:N → Usuario |
| Usuario | Autenticación y control de acceso | N:1 ← Rol |
| Cliente | Catálogo de clientes | 1:N → Vehículo, 1:N → Venta |
| Vehículo | Vehículos de clientes (máx 2 activos) | N:1 ← Cliente |
| Producto | Catálogo de materiales | 1:N → DetalleVenta, 1:N → LoteInventario |
| Pesaje | Lecturas de báscula RS232 | - |
| Venta | Transacciones principales | 1:N → DetalleVenta, 1:1 → Factura |
| DetalleVenta | Líneas de venta | N:1 ← Venta, N:1 ← Producto |
| Factura | Comprobantes formales | 1:1 ← Venta |
| MovimientoInventario | Auditoría de cambios de stock | - |
| LoteInventario | Control de lotes | N:1 ← Producto |
| Auditoria | Trazabilidad de cambios | - |

---

### B. `GestionComercial.Application` (Capa de Aplicación)
- **TargetFramework:** net8.0
- **Nullable:** enabled
- **Propósito:** Lógica de negocio, DTOs, interfaces de repositorios
- **Dependencias:**
  - Mapster 7.4.0 (mapping de objetos)
  - Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0
  - Microsoft.Extensions.Logging.Abstractions 8.0.0

**Contenido:**
```
DTOs/
├── UsuarioDTO.cs
│   ├── UsuarioDTO (lectura)
│   ├── CrearActualizarUsuarioDTO
│   ├── LoginDTO
│   └── LoginResponseDTO
│
Interfaces/
├── IRepository<T>           ← Patrón genérico
├── IClienteRepository
├── IUsuarioRepository
├── IVehiculoRepository
│
Services/
├── IUsuarioService          ← Autenticación
├── ClienteService
├── UsuarioService
└── VentaService
```

---

### C. `GestionComercial.Infrastructure` (Capa de Infraestructura)
- **TargetFramework:** net8.0
- **Nullable:** enabled
- **Propósito:** Acceso a datos, Entity Framework, repositorios
- **Dependencias:**
  - Microsoft.EntityFrameworkCore 8.0.0
  - Microsoft.EntityFrameworkCore.SqlServer 8.0.0
  - Microsoft.EntityFrameworkCore.Design 8.0.0
  - Microsoft.Extensions.DependencyInjection 8.0.0
  - Serilog 3.1.0
  - Serilog.Sinks.File 5.0.0

**Contenido:**
```
DependencyInjection.cs
├── Registra DbContext
├── Repositorio genérico
├── Repositorios especializados (11 total)
└── Servicios de aplicación

Persistence/
├── GestionComercialContext.cs
│   ├── 12 DbSets
│   ├── Configuración Fluent API
│   ├── Índices
│   └── Constraints
│
├── Migrations/
│   └── [Migraciones de EF Core]
│
└── Repositories/
    ├── Repository.cs           ← Patrón genérico CRUD
    ├── AuditoriaRepository.cs
    ├── ClienteRepository.cs
    ├── FacturaRepository.cs
    ├── LoteInventarioRepository.cs
    ├── MovimientoInventarioRepository.cs
    ├── PesajeRepository.cs
    ├── ProductoRepository.cs
    ├── UsuarioRepository.cs
    ├── VehiculoRepository.cs
    └── VentaRepository.cs
```

---

### D. `GestionComercial.Presentation` (Capa de Presentación - WPF)
- **TargetFramework:** net8.0-windows
- **OutputType:** WinExe (Ejecutable de Windows)
- **Propósito:** Interfaz de usuario WPF con patrón MVVM
- **UseWPF:** true

**Dependencias:**
- CommunityToolkit.Mvvm 8.2.2 (Observable properties, RelayCommand)
- Microsoft.Extensions.DependencyInjection 8.0.0
- Microsoft.Extensions.Configuration 8.0.0
- Microsoft.Extensions.Configuration.Json 8.0.0
- Microsoft.Extensions.Hosting 8.0.0
- Serilog 3.1.1 (Logging)
- Serilog.Extensions.Hosting 8.0.0
- Serilog.Sinks.Console 5.0.0
- Serilog.Sinks.Debug 2.0.0
- Serilog.Sinks.File 5.0.0

---

### E. `GestionComercial.Tests` (Pruebas)
- **TargetFramework:** net8.0
- **Propósito:** Pruebas unitarias (xUnit - futuro)
- **Estado:** Estructura creada, sin tests implementados

---

## 3. VIEWMODELS (PRESENTATION LAYER)

### Raíz de ViewModels

#### A. **LoginViewModel.cs**
```
Clase: LoginViewModel : ObservableObject
Propósito: Gestiona la autenticación del usuario

PROPIEDADES:
- NombreUsuario: string                    ← Input bindeable
- Contrasena: string                       ← Contraseña (no bindeable por seguridad)
- MensajeError: string?                    ← Mostrar errores
- EstaCargando: bool                       ← Estado de carga

COMANDOS:
- IniciarSesionAsync()                     ← Ejecuta AutenticarAsync

EVENTOS:
- LoginExitoso: Action<LoginResponseDTO>   ← Se invoca al autenticar

DEPENDENCIAS INYECTADAS:
- IUsuarioService _usuarioService          ← Autentica usuario

FLUJO:
1. Usuario ingresa credenciales
2. IniciarSesionAsync() valida entrada
3. Llamada a _usuarioService.AutenticarAsync(LoginDTO)
4. Si es exitoso → LoginExitoso.Invoke(LoginResponseDTO)
5. Si falla → MensajeError muestra motivo
```

**Últimas líneas importantes:**
```csharp
// Línea 46-50: Invoca el evento de login exitoso
LoginExitoso?.Invoke(resultado);

// Línea 55: Captura errores de conexión BD
catch
{
    MensajeError = "No se pudo conectar a la base de datos...";
}
```

---

#### B. **ShellViewModel.cs**
```
Clase: ShellViewModel : ObservableObject
Propósito: Gestiona la ventana principal y navegación entre módulos

PROPIEDADES DE ESTADO:
- CurrentViewModel: ObservableObject?      ← ViewModel actual del módulo
- UsuarioNombre: string                    ← Nombre de usuario autenticado
- UsuarioRol: string                       ← Rol del usuario
- TituloModulo: string                     ← Título dinámico del módulo
- [Bool]Seleccionado: 7 flags              ← Estados de botones (Dashboard, Ventas, etc)

COMANDOS DE NAVEGACIÓN:
- IrDashboard()                            ← Carga DashboardViewModel
- IrVentas()                               ← Carga VentasViewModel
- IrPesajes()                              ← Carga PesajesViewModel
- IrClientes()                             ← Carga ClientesViewModel
- IrInventario()                           ← Carga InventarioViewModel
- IrFacturas()                             ← Carga FacturasViewModel
- IrUsuarios()                             ← Carga UsuariosViewModel
- CerrarSesion()                           ← Cierra sesión

MÉTODOS:
- Inicializar(LoginResponseDTO usuario)    ← Inicia sesión en la app
- LimpiarSeleccion()                       ← Reinicia flags de botones

EVENTOS:
- SolicitudCerrarSesion: Action            ← Señal para cerrar ventana login

DEPENDENCIAS:
- IServiceProvider _services               ← Resuelve ViewModels
- SessionService _session                  ← Información de sesión

FLUJO:
1. MainShellWindow.xaml.cs llama a Inicializar(usuario)
2. Se inicia SessionService con datos del usuario
3. IirDashboard() se ejecuta automáticamente
4. Cada comando de navegación:
   - Limpia selección anterior
   - Marca nuevo botón como seleccionado
   - Resuelve ViewModel correspondiente
   - Asigna a CurrentViewModel (binding actualiza UI)
```

---

### Módulos de ViewModels (Subdirectorio: ViewModels\Modules\)

#### C. **DashboardViewModel.cs**
```
Clase: DashboardViewModel : ObservableObject
Propósito: Pantalla principal con métricas rápidas

PROPIEDADES:
- TotalVentasHoy: int                      ← Contador (0 por ahora)
- TotalClientesActivos: int                ← Contador (0 por ahora)
- PesajesHoy: int                          ← Contador (0 por ahora)
- AlertasStock: int                        ← Contador (0 por ahora)
- Bienvenida: string                       ← "Bienvenido, [Usuario]"
- EstaCargando: bool                       ← Indicador de carga

COMANDOS:
- CargarDatosAsync()                       ← TODO: Conectar repositorios

DEPENDENCIAS:
- IUsuarioService _usuarioService          ← Futuro: consultas
- SessionService _session                  ← Obtener usuario actual

ESTADO ACTUAL:
❌ Valores hardcodeados en 0
❌ CargarDatosAsync() es TODO
✅ UI binding funciona correctamente
```

---

#### D. **VentasViewModel.cs**
```
Clase: VentasViewModel : ObservableObject
Propósito: Gestión de transacciones de venta

ESTADO: Estructura base creada
⏳ Sin lógica implementada
```

---

#### E. **ClientesViewModel.cs**
```
Clase: ClientesViewModel : ObservableObject
Propósito: Gestión de clientes y vehículos asociados

ESTADO: Estructura base creada
⏳ Sin lógica implementada
```

---

#### F. **InventarioViewModel.cs**
```
Clase: InventarioViewModel : ObservableObject
Propósito: Control de stock y lotes de inventario

ESTADO: Estructura base creada
⏳ Sin lógica implementada
```

---

#### G. **PesajesViewModel.cs**
```
Clase: PesajesViewModel : ObservableObject
Propósito: Lectura de báscula RS232 y registro de pesajes

ESTADO: Estructura base creada
⏳ Sin integración RS232
```

---

#### H. **FacturasViewModel.cs**
```
Clase: FacturasViewModel : ObservableObject
Propósito: Gestión de facturación

ESTADO: Estructura base creada
⏳ Sin lógica de generación de facturas
```

---

#### I. **UsuariosViewModel.cs**
```
Clase: UsuariosViewModel : ObservableObject
Propósito: Administración de usuarios y roles

ESTADO: Estructura base creada
⏳ Sin lógica de CRUD
```

---

## 4. VIEWS/XAML (PRESENTATION LAYER)

### Raíz de Views

#### A. **LoginWindow.xaml / LoginWindow.xaml.cs**

**LoginWindow.xaml (UI):**
- TextBox para NombreUsuario (binding)
- PasswordBox para Contrasena (no bindeable por seguridad)
- TextBlock para mostrar MensajeError
- Button para IniciarSesion (RelayCommand)
- ProgressRing cuando EstaCargando = true
- Diseño profesional con colores corporativos

**LoginWindow.xaml.cs (Code-behind):**
```csharp
// Bindings:
- DataContext = LoginViewModel
- PasswordBox → LoginViewModel.Contrasena (manual en code-behind)

// Eventos:
- LoginExitoso → Abre MainShellWindow y cierra LoginWindow
```

---

#### B. **MainShellWindow.xaml / MainShellWindow.xaml.cs**

**MainShellWindow.xaml (UI):**
- Barra lateral con 7 botones de navegación
  - Dashboard
  - Ventas
  - Pesajes
  - Clientes
  - Inventario
  - Facturas
  - Usuarios
- Área superior con nombre de usuario y rol
- Botón de Cerrar Sesión
- ContentControl que carga CurrentViewModel dinámicamente
- Estilos WPF aplicados

**MainShellWindow.xaml.cs (Code-behind):**
```csharp
// Bindings:
- DataContext = ShellViewModel

// Métodos:
- Constructor recibe LoginResponseDTO del LoginWindow
- Llama a ShellViewModel.Inicializar(usuario)
```

---

### Módulos de Views (Subdirectorio: Views\Modules\)

#### C. **DashboardView.xaml**

**Estado:**
✅ Archivo existe  
⏳ Contenido: Estructura base para dashboard  
❌ Componentes sin completar

**Binding:**
- Debería bindear a DashboardViewModel
- TextBlocks para mostrar totales
- Indicadores visuales (colores, iconos)

---

## 5. CONFIGURACIÓN DE LA APLICACIÓN

### A. **App.xaml**

**Secciones:**

1. **Declaraciones de Namespaces:**
```xml
xmlns:vm="clr-namespace:GestionComercial.Presentation.ViewModels.Modules"
xmlns:mod="clr-namespace:GestionComercial.Presentation.Views.Modules"
```

2. **Colores Base (ResourceDictionary):**
```
PrimaryBrush: #1E3A5F (azul oscuro)
PrimaryHoverBrush: #2A4E7F
AccentBrush: #E8A020 (naranja/dorado)
BackgroundBrush: #F0F2F5
CardBrush: #FFFFFF
TextPrimaryBrush: #1A1A2E
TextMutedBrush: #6B7280
DangerBrush: #EF4444 (rojo)
SuccessBrush: #10B981 (verde)
WarningBrush: #F59E0B (amarillo)
```

3. **Estilos Globales:**
- InputStyle para TextBox (altura 40px, radio 6px, bordes dinámicos)

4. **Startup:**
```xml
Startup="Application_Startup"
```

---

### B. **App.xaml.cs**

**Punto de Entrada:**
```csharp
Método: Application_Startup(object sender, StartupEventArgs e)
```

**Configuración de Logging con Serilog:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/GestionComercial-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```
- Logs diarios en carpeta "logs/"
- Nivel mínimo: Debug

**Configuración de Host (.NET Generic Host):**
```csharp
_host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(...)      ← Carga appsettings.json
    .ConfigureServices(...)              ← Registro de servicios
    .UseSerilog()                        ← Integra Serilog
    .Build();
```

**Registro de Servicios en DI Container:**

1. **Infraestructura:**
   ```csharp
   services.AddInfrastructure(context.Configuration);
   ```
   - DbContext
   - Repositorios (11 total)
   - Servicios de aplicación

2. **Sesión Global:**
   ```csharp
   services.AddSingleton<SessionService>();
   ```

3. **ViewModels (Transient):**
   - LoginViewModel
   - ShellViewModel
   - DashboardViewModel
   - VentasViewModel
   - PesajesViewModel
   - ClientesViewModel
   - InventarioViewModel
   - FacturasViewModel
   - UsuariosViewModel

4. **Ventanas (Singleton):**
   - LoginWindow
   - MainShellWindow

**Flujo de Inicio:**
```
1. _host.Start()
2. Resuelve LoginWindow desde DI container
3. loginWindow.Show()
4. OnExit: _host.StopAsync() y Log.CloseAndFlush()
```

---

### C. **appsettings.json**

**Secciones Configurables:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DSKTOP_DSAMCJEZ\\SQLEXPRESS;Database=GestionComercial_BD;User Id=sa;Password=xDfsc.2026-;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "LocalSQLite": "Data Source=GestionComercial.db"
  },
  
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "GestionComercial": "Debug"
    }
  },
  
  "ApplicationSettings": {
    "AppName": "Sistema de Gestión Comercial",
    "Version": "1.0.0",
    "Environment": "Development"
  },
  
  "SerialPort": {
    "PortName": "COM1",
    "BaudRate": 9600,
    "DataBits": 8,
    "StopBits": "1",
    "Parity": "None",
    "ReadTimeout": 5000,
    "WriteTimeout": 5000
  },
  
  "Security": {
    "PasswordHashIterations": 10,
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 30,
    "PasswordExpirationDays": 60
  }
}
```

**Claves Importantes:**
- ✅ Cadena de conexión a SQL Server configurada
- ✅ Puerto COM1 para báscula RS232 (configurable)
- ✅ Seguridad: hash iterations, intentos máximos, expiración de contraseña
- ⚠️ **CAMBIAR:** Servidor, credenciales de BD en producción

---

## 6. INFRAESTRUCTURA Y PERSISTENCIA

### A. **GestionComercialContext.cs**

**Hereda de:** DbContext (Entity Framework Core)

**DbSets (12 total):**
```csharp
DbSet<Rol>
DbSet<Usuario>
DbSet<Cliente>
DbSet<Vehiculo>
DbSet<Producto>
DbSet<Pesaje>
DbSet<Venta>
DbSet<DetalleVenta>
DbSet<Factura>
DbSet<MovimientoInventario>
DbSet<LoteInventario>
DbSet<Auditoria>
```

**Configuración OnModelCreating:**

Ejemplo de Rol:
```csharp
entity.HasKey(e => e.ID_Rol);
entity.Property(e => e.Nombre)
    .IsRequired()
    .HasMaxLength(50);
entity.HasIndex(e => e.Nombre).IsUnique();
```

Ejemplo de Usuario:
```csharp
entity.HasKey(e => e.ID_Usuario);
entity.HasIndex(e => e.NombreUsuario).IsUnique();
entity.HasOne(e => e.Rol)
    .WithMany(r => r.Usuarios)
    .HasForeignKey(e => e.ID_Rol)
    .OnDelete(DeleteBehavior.Restrict);
```

Configuraciones Aplicadas:
- ✅ Primary Keys
- ✅ Unique Constraints
- ✅ Índices
- ✅ Foreign Keys con DeleteBehavior
- ✅ MaxLength en strings
- ✅ Required/Optional

---

### B. **DependencyInjection.cs**

**Método:** `AddInfrastructure(IServiceCollection services, IConfiguration configuration)`

**Registros:**

1. **DbContext:**
   ```csharp
   services.AddDbContext<GestionComercialContext>(options =>
       options.UseSqlServer(connectionString, sqlOptions =>
       {
           sqlOptions.MigrationsAssembly(...);
           sqlOptions.CommandTimeout(30);
       }));
   ```

2. **Repositorio Genérico:**
   ```csharp
   services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
   ```

3. **Repositorios Especializados (Scoped):**
   - IUsuarioRepository → UsuarioRepository
   - IClienteRepository → ClienteRepository
   - IVehiculoRepository → VehiculoRepository
   - IRepository<Producto> → ProductoRepository
   - IRepository<Venta> → VentaRepository
   - IRepository<Pesaje> → PesajeRepository
   - IRepository<Factura> → FacturaRepository
   - IRepository<MovimientoInventario> → MovimientoInventarioRepository
   - IRepository<LoteInventario> → LoteInventarioRepository
   - IRepository<Auditoria> → AuditoriaRepository

4. **Servicios de Aplicación (Scoped):**
   - IUsuarioService → UsuarioService
   - IClienteService → ClienteService
   - (VentaService - se registraría aquí)

---

### C. **Repositorios**

**Estructura Patrón Repository Pattern:**

1. **Repository.cs (Genérico):**
   ```csharp
   public class Repository<T> : IRepository<T> where T : class
   {
       protected GestionComercialContext _context;
       
       // CRUD base:
       // GetByIdAsync(id)
       // GetAllAsync()
       // AddAsync(entity)
       // UpdateAsync(entity)
       // DeleteAsync(entity)
       // SaveChangesAsync()
   }
   ```

2. **Repositorios Especializados:**

   - **UsuarioRepository.cs:**
     ```csharp
     public interface IUsuarioRepository : IRepository<Usuario>
     {
         Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
         Task<Usuario?> ObtenerPorIdAsync(int id);
         Task<bool> VerificarContraseñaAsync(Usuario usuario, string contrasena);
     }
     ```

   - **ClienteRepository.cs:**
     - Hereda: `Repository<Cliente>, IClienteRepository`
     - Métodos especiales: búsqueda por número de identificación, etc.

   - **VehiculoRepository.cs:**
     - IVehiculoRepository
     - Validaciones: máximo 2 vehículos activos por cliente

   - Los demás siguen el mismo patrón

---

### D. **Migraciones**

**Ubicación:** `GestionComercial.Infrastructure/Persistence/Migrations/`

**Estado:**
- ✅ Estructura preparada
- ⏳ Primera migración pendiente (dotnet ef migrations add InitialCreate)
- ⏳ Apply: dotnet ef database update

---

## 7. CAPA DE APLICACIÓN

### A. **DTOs (Data Transfer Objects)**

**UsuarioDTO.cs:**

```csharp
// 1. Lectura
public class UsuarioDTO
{
    public int ID_Usuario
    public string NombreUsuario
    public string NombreCompleto
    public string Email
    public string Telefono
    public int ID_Rol
    public string RolNombre
    public string Estado
    public DateTime? UltimoLogin
    public DateTime FechaCreacion
}

// 2. Crear/Actualizar
public class CrearActualizarUsuarioDTO
{
    public string NombreUsuario
    public string NombreCompleto
    public string Email
    public string? Telefono
    public int ID_Rol
    public string? Contrasena  // Solo creación
}

// 3. Login
public class LoginDTO
{
    public string NombreUsuario
    public string Contrasena
}

// 4. Respuesta Login
public class LoginResponseDTO
{
    public int ID_Usuario
    public string NombreUsuario
    public string NombreCompleto
    public int ID_Rol
    public string RolNombre
    public string Token  // JWT (futuro)
}
```

**ServiceDTOs.cs:**
- DTOs genéricos (por expandir)

---

### B. **Interfaces**

**IRepository.cs (Genérico):**
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
```

**IUsuarioRepository.cs (Especializado):**
```csharp
public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<bool> VerificarContraseñaAsync(Usuario usuario, string contrasena);
}
```

Interfaces similares para:
- IClienteRepository
- IVehiculoRepository

---

### C. **Servicios**

**IUsuarioService.cs:**
```csharp
public interface IUsuarioService
{
    Task<LoginResponseDTO?> AutenticarAsync(LoginDTO credenciales);
    Task<UsuarioDTO?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<UsuarioDTO>> ObtenerTodosAsync();
    Task<bool> CrearUsuarioAsync(CrearActualizarUsuarioDTO dto);
    Task<bool> ActualizarUsuarioAsync(int id, CrearActualizarUsuarioDTO dto);
    Task<bool> EliminarUsuarioAsync(int id);
}
```

**UsuarioService.cs:**
- Implementa IUsuarioService
- Inyecta IUsuarioRepository
- Gestiona autenticación y CRUD de usuarios
- Hash de contraseñas (usando KeyDerivation)

**ClienteService.cs:**
- CRUD de clientes
- Validaciones de negocio

**VentaService.cs:**
- Procesamiento de transacciones
- Cálculo de totales, descuentos

---

## 8. DEPENDENCIAS Y NUGET PACKAGES

### A. **GestionComercial.Domain**
```
✓ Microsoft.AspNetCore.Cryptography.KeyDerivation 8.0.0
```

### B. **GestionComercial.Application**
```
✓ Mapster 7.4.0
✓ Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0
✓ Microsoft.Extensions.Logging.Abstractions 8.0.0
```

### C. **GestionComercial.Infrastructure**
```
✓ Microsoft.EntityFrameworkCore 8.0.0
✓ Microsoft.EntityFrameworkCore.SqlServer 8.0.0
✓ Microsoft.EntityFrameworkCore.Design 8.0.0
✓ Microsoft.Extensions.DependencyInjection 8.0.0
✓ Serilog 3.1.0
✓ Serilog.Sinks.File 5.0.0
```

### D. **GestionComercial.Presentation (WPF)**
```
✓ CommunityToolkit.Mvvm 8.2.2
✓ Microsoft.Extensions.DependencyInjection 8.0.0
✓ Microsoft.Extensions.Configuration 8.0.0
✓ Microsoft.Extensions.Configuration.Json 8.0.0
✓ Microsoft.Extensions.Hosting 8.0.0
✓ Serilog 3.1.1
✓ Serilog.Extensions.Hosting 8.0.0
✓ Serilog.Sinks.Console 5.0.0
✓ Serilog.Sinks.Debug 2.0.0
✓ Serilog.Sinks.File 5.0.0
```

### E. **Tecnología Subyacente**
```
✓ .NET 8.0 (LTS)
✓ Entity Framework Core 8.0
✓ WPF (Windows Presentation Foundation)
✓ Dependency Injection (.NET Native)
✓ Configuration System (.NET Native)
✓ Hosting Model (.NET Generic Host)
```

---

## 9. FLUJO DE INICIALIZACIÓN

### Paso a Paso de Ejecución:

```
1. INICIO DE APLICACIÓN
   └─> Program.cs ejecuta
   └─> App.xaml.cs → Application_Startup(...)

2. CONFIGURACIÓN DE LOGGING (Serilog)
   └─> Crea carpeta "logs/"
   └─> Archivo: GestionComercial-[fecha].txt
   └─> Nivel: Debug

3. CREACIÓN DE HOST (.NET Generic Host)
   ├─> ConfigureAppConfiguration()
   │   └─> Lee appsettings.json
   │
   ├─> ConfigureServices()
   │   ├─> AddInfrastructure(configuration)
   │   │   ├─> DbContext con SQL Server
   │   │   ├─> Repositorio genérico
   │   │   ├─> 10 Repositorios especializados
   │   │   └─> 2 Servicios (Usuario, Cliente)
   │   │
   │   ├─> SessionService (Singleton)
   │   │
   │   ├─> ViewModels (Transient)
   │   │   ├─> LoginViewModel
   │   │   ├─> ShellViewModel
   │   │   └─> 7 Módulos ViewModel
   │   │
   │   └─> Ventanas (Singleton)
   │       ├─> LoginWindow
   │       └─> MainShellWindow
   │
   └─> UseSerilog()

4. INICIO DE HOST
   └─> _host.Start()

5. MOSTRAR LoginWindow
   └─> var loginWindow = _host.Services.GetRequiredService<LoginWindow>()
   └─> loginWindow.Show()

6. USUARIO INGRESA CREDENCIALES
   └─> LoginWindow.xaml → LoginViewModel.IniciarSesionAsync()
   └─> Llama a IUsuarioService.AutenticarAsync(LoginDTO)
   └─> UsuarioService.cs → UsuarioRepository.ObtenerPorNombreUsuarioAsync()
   └─> Verifica contraseña con KeyDerivation

7. SI AUTENTICACIÓN EXITOSA
   └─> LoginViewModel.LoginExitoso?.Invoke(LoginResponseDTO)
   └─> LoginWindow.xaml.cs recibe evento
   └─> Abre MainShellWindow(usuario)
   └─> Cierra LoginWindow

8. MAINSHELLWINDOW ABIERTO
   └─> ShellViewModel.Inicializar(LoginResponseDTO)
   └─> SessionService.IniciarSesion(usuario)
   └─> ShellViewModel.IrDashboard()
   └─> Carga DashboardViewModel

9. NAVEGACIÓN POR MÓDULOS
   └─> Usuario hace clic en botón
   └─> ShellViewModel ejecuta comando (IrVentas, IrClientes, etc)
   └─> Resuelve ViewModel desde DI container
   └─> Asigna a CurrentViewModel
   └─> Binding en MainShellWindow.xaml actualiza UI

10. CIERRE DE SESIÓN
    └─> Usuario hace clic en Cerrar Sesión
    └─> ShellViewModel.CerrarSesion()
    └─> SessionService.CerrarSesion()
    └─> SolicitudCerrarSesion?.Invoke()
    └─> MainShellWindow.xaml.cs cierra ventana
    └─> Muestra LoginWindow nuevamente

11. CIERRE DE APLICACIÓN
    └─> App.OnExit(ExitEventArgs)
    └─> _host.StopAsync().GetAwaiter().GetResult()
    └─> Log.CloseAndFlush()
```

---

## 10. ESTADO ACTUAL Y PRÓXIMOS PASOS

### ✅ COMPLETADO (22-04-2026)

**Arquitectura y Estructura:**
- ✅ Clean Architecture (4 capas)
- ✅ MVVM pattern en presentación
- ✅ Dependency Injection configurado
- ✅ Entity Framework Core DbContext
- ✅ 11 Repositorios implementados

**Base de Datos:**
- ✅ Script SQL inicial (13 tablas)
- ✅ Triggers y constraints
- ✅ Índices optimizados
- ✅ 4 vistas útiles

**Presentación:**
- ✅ App.xaml con estilos globales
- ✅ LoginWindow UI y lógica
- ✅ MainShellWindow UI y navegación
- ✅ 7 módulos (ViewModels + estructura Views)
- ✅ SessionService para sesión de usuario

**Configuración:**
- ✅ appsettings.json con todas las secciones
- ✅ Logging configurado (Serilog)
- ✅ Puerto COM1 para báscula
- ✅ Seguridad: hash iterations, lockout, expiración

---

### ⏳ PRÓXIMOS PASOS (Por Hacer)

#### FASE 1: Base de Datos
1. ⏳ Ejecutar migrations EF: `dotnet ef migrations add InitialCreate`
2. ⏳ Aplicar: `dotnet ef database update`
3. ⏳ Ejecutar SCRIPT_BASE_DATOS.sql (datos iniciales)

#### FASE 2: Servicios y Lógica
4. ⏳ Implementar UsuarioService.AutenticarAsync() completo
5. ⏳ Implementar ClienteService CRUD
6. ⏳ Implementar VentaService (cálculos)
7. ⏳ Integración RS232 para báscula (COM1)

#### FASE 3: ViewModels y Views
8. ⏳ Completar DashboardViewModel (conectar datos)
9. ⏳ Implementar VentasViewModel + VentasView
10. ⏳ Implementar ClientesViewModel + ClientesView
11. ⏳ Implementar InventarioViewModel + InventarioView
12. ⏳ Implementar PesajesViewModel + PesajesView
13. ⏳ Implementar FacturasViewModel + FacturasView
14. ⏳ Implementar UsuariosViewModel + UsuariosView

#### FASE 4: Testing
15. ⏳ Tests unitarios con xUnit
16. ⏳ Cobertura de servicios críticos

#### FASE 5: Features Avanzados
17. ⏳ Autenticación JWT (token)
18. ⏳ Caché de datos offline-first
19. ⏳ Sincronización con servidor
20. ⏳ Reportes PDF

---

### 📊 MATRIZ DE ESTADO POR MÓDULO

| Módulo | ViewModel | View | Datos | Interfaz | Estado |
|--------|-----------|------|-------|----------|--------|
| Dashboard | ✅ | ⏳ | ❌ | ⏳ | Estructura base |
| Ventas | ✅ | ❌ | ❌ | ❌ | Planificado |
| Pesajes (Báscula) | ✅ | ❌ | ❌ | ❌ | RS232 pendiente |
| Clientes | ✅ | ❌ | ❌ | ❌ | Planificado |
| Inventario | ✅ | ❌ | ❌ | ❌ | Planificado |
| Facturas | ✅ | ❌ | ❌ | ❌ | Planificado |
| Usuarios | ✅ | ❌ | ❌ | ❌ | Planificado |
| Autenticación | ✅ | ✅ | ⏳ | ✅ | En progreso |

---

### 🔑 CREDENCIALES DE PRUEBA

```
Base de Datos:
- Server: DSKTOP_DSAMCJEZ\SQLEXPRESS
- Database: GestionComercial_BD
- User: sa
- Password: xDfsc.2026-
- ⚠️ CAMBIAR EN PRODUCCIÓN

Usuario ADMIN:
- Usuario: admin
- Contraseña: 123456
- Rol: Admin
- ⚠️ CAMBIAR EN PRIMERA EJECUCIÓN

Usuario OPERADOR:
- Usuario: operador
- Contraseña: 123456
- Rol: Operador
- ⚠️ CAMBIAR EN PRIMERA EJECUCIÓN
```

---

### 📝 NOTAS IMPORTANTES

1. **Offline-First:** Base de datos local, preparada para sincronización
2. **Seguridad:** Contraseñas hasheadas con KeyDerivation
3. **Auditoría:** Todas las tablas tienen timestamps
4. **Escalabilidad:** Estructura lista para migración a web
5. **Logging:** Todos los eventos se registran en logs/
6. **Validación:** Constraints en DB + validación en aplicación
7. **Concurrencia:** MultipleActiveResultSets=true en conexión
8. **Performance:** Índices optimizados en claves primarias y búsquedas frecuentes

---

**Documento Actualizado:** 22 de Abril de 2026  
**Versión:** 1.0  
**Responsable:** Sistema de Gestión Comercial Team
