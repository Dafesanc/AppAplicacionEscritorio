# 🏗️ ESTRUCTURA DEL PROYECTO .NET

## Proyectos Creados

### 1. **GestionComercial.Domain** (Class Library)
Capa de dominio - Contiene las entidades de negocio

```
Domain/
├── Entities/
│   ├── Rol.cs
│   ├── Usuario.cs
│   ├── Cliente.cs
│   ├── Vehiculo.cs
│   ├── Producto.cs
│   ├── Pesaje.cs
│   ├── Venta.cs
│   ├── DetalleVenta.cs
│   ├── Factura.cs
│   ├── MovimientoInventario.cs
│   ├── LoteInventario.cs
│   └── Auditoria.cs
├── Interfaces/
│   └── (A completar: IEntity, IAggregateRoot, etc.)
└── GestionComercial.Domain.csproj
```

**Responsabilidades:**
- ✅ Definir entidades del negocio
- ✅ Lógica pura de dominio
- ✅ No depende de ningún framework externo
- ✅ Valida reglas de negocio

**Entidades Creadas:**

| Entidad | Propósito | Relaciones |
|---------|----------|-----------|
| Rol | Diccionario de roles | 1 → Muchos Usuarios |
| Usuario | Autenticación | 1 Rol, Muchas Auditorías |
| Cliente | Catálogo de clientes | Muchos Vehículos, Ventas |
| Vehiculo | Vehículos de cliente | 1 Cliente, Muchos Pesajes |
| Producto | Catálogo de productos | Muchas Ventas (DetalleVenta) |
| Pesaje | Registro desde báscula | 1 Vehículo |
| Venta | Transacciones de venta | 1 Cliente, Muchas Líneas |
| DetalleVenta | Línea de venta | 1 Venta, 1 Producto |
| Factura | Comprobante formal | 1 Venta, 1 Cliente |
| MovimientoInventario | Auditoría de stock | 1 Producto |
| LoteInventario | Control de lotes | 1 Producto |
| Auditoria | Trazabilidad | 1 Usuario |

---

### 2. **GestionComercial.Application** (Class Library)
Capa de aplicación - Casos de uso y servicios

```
Application/
├── Interfaces/
│   ├── IRepository.cs (genérica)
│   ├── IUsuarioRepository.cs
│   ├── IClienteRepository.cs
│   ├── IVentaRepository.cs
│   ├── IVentasService.cs
│   ├── IUsuariosService.cs
│   └── (A completar)
├── Services/
│   ├── UsuariosService.cs
│   ├── ClientesService.cs
│   ├── VentasService.cs
│   ├── InventarioService.cs
│   ├── PesajeService.cs
│   └── (A completar)
├── DTOs/
│   ├── UsuarioDTO.cs
│   ├── ClienteDTO.cs
│   ├── VentaDTO.cs
│   └── (A completar)
├── Exceptions/
│   ├── DomainException.cs
│   ├── UsuarioException.cs
│   ├── VentaException.cs
│   └── (A completar)
├── Mappings/
│   └── MappingProfile.cs (Mapster)
└── GestionComercial.Application.csproj
```

**Responsabilidades:**
- ✅ Casos de uso de la aplicación
- ✅ Servicios de negocio
- ✅ DTOs para comunicación
- ✅ Validaciones de aplicación
- ✅ No accede directamente a BD

**Dependencias:**
```
Application → Domain
```

---

### 3. **GestionComercial.Infrastructure** (Class Library)
Capa de infraestructura - Acceso a datos, logging, externos

```
Infrastructure/
├── Persistence/
│   ├── GestionComercialContext.cs (DbContext)
│   ├── Repositories/
│   │   ├── Repository.cs (base genérica)
│   │   ├── UsuarioRepository.cs
│   │   ├── ClienteRepository.cs
│   │   ├── VentaRepository.cs
│   │   └── (A completar)
│   ├── Configuration/
│   │   ├── EntityTypeConfiguration.cs (base)
│   │   ├── UsuarioConfiguration.cs
│   │   ├── ClienteConfiguration.cs
│   │   └── (A completar)
│   └── Migrations/
│       └── (Generadas por EF Core)
├── Serial/
│   ├── ISerialPortAdapter.cs
│   ├── SerialPortAdapter.cs
│   ├── PesajeReader.cs
│   └── WeightValidator.cs
├── Logging/
│   ├── FileLogger.cs
│   └── LoggerService.cs
├── DependencyInjection.cs (Configuración de DI)
└── GestionComercial.Infrastructure.csproj
```

**Responsabilidades:**
- ✅ DbContext de Entity Framework
- ✅ Implementación de Repositories
- ✅ Migraciones de BD
- ✅ Communicación RS232 (báscula)
- ✅ Logging
- ✅ Caché (futuro)

**Dependencias:**
```
Infrastructure → Domain
Infrastructure → Application
```

---

### 4. **GestionComercial.Presentation** (WPF Application)
Capa de presentación - Interfaz de usuario

```
Presentation/
├── App.xaml / App.xaml.cs
├── appsettings.json
├── GlobalUsings.cs
├── Startup.cs / ServiceConfiguration.cs
│
├── Views/
│   ├── LoginWindow.xaml / .xaml.cs
│   ├── MainWindow.xaml / .xaml.cs
│   ├── Dashboard/
│   │   ├── DashboardWindow.xaml
│   │   └── DashboardWindow.xaml.cs
│   ├── Ventas/
│   │   ├── VentaWindow.xaml
│   │   ├── DetalleVentaDialog.xaml
│   │   └── ...
│   ├── Clientes/
│   │   ├── ClientesWindow.xaml
│   │   ├── NuevoClienteDialog.xaml
│   │   └── ...
│   ├── Inventario/
│   │   ├── InventarioWindow.xaml
│   │   ├── ProductosWindow.xaml
│   │   └── ...
│   ├── Reportes/
│   │   ├── ReportesWindow.xaml
│   │   ├── VentasPorPeriodo.xaml
│   │   └── ...
│   └── Configuracion/
│       ├── ConfiguracionWindow.xaml
│       ├── UsuariosWindow.xaml
│       └── ...
│
├── ViewModels/
│   ├── Base/
│   │   ├── BaseViewModel.cs
│   │   └── RelayCommand.cs
│   ├── LoginViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── VentaViewModel.cs
│   ├── ClientesViewModel.cs
│   ├── InventarioViewModel.cs
│   ├── ReportesViewModel.cs
│   └── ...
│
├── Converters/
│   ├── DateConverter.cs
│   ├── BoolToVisibilityConverter.cs
│   ├── EstadoColorConverter.cs
│   └── ...
│
├── Resources/
│   ├── Strings.xaml (recursos de texto)
│   ├── Styles.xaml (estilos globales)
│   └── Colors.xaml (paleta de colores)
│
├── Services/
│   ├── NavigationService.cs
│   ├── DialogService.cs
│   └── SessionService.cs
│
└── GestionComercial.Presentation.csproj
```

**Responsabilidades:**
- ✅ Interfaz de usuario en WPF
- ✅ ViewModels (MVVM)
- ✅ Navegación entre pantallas
- ✅ Gestión de sesión
- ✅ Diálogos y notificaciones

**Dependencias:**
```
Presentation → Domain
Presentation → Application
Presentation → Infrastructure
```

**Patrón Arquitectónico:**
- MVVM (Model-View-ViewModel)
- XAML para UI
- Data Binding para reactividad
- ICommand para acciones

---

### 5. **GestionComercial.Tests** (xUnit Test Project)
Capa de pruebas - Tests unitarios e integración

```
Tests/
├── Unit/
│   ├── Domain/
│   │   ├── UsuarioTests.cs
│   │   ├── ClienteTests.cs
│   │   └── VentaTests.cs
│   ├── Application/
│   │   ├── UsuariosServiceTests.cs
│   │   ├── VentasServiceTests.cs
│   │   └── ...
│   └── Presentation/
│       ├── LoginViewModelTests.cs
│       ├── VentaViewModelTests.cs
│       └── ...
│
├── Integration/
│   ├── RepositoryTests.cs
│   ├── DatabaseTests.cs
│   └── ...
│
├── E2E/
│   ├── VentaCompleteFlowTests.cs
│   └── ...
│
├── Fixtures/
│   ├── DatabaseFixture.cs
│   └── TestDataBuilder.cs
│
└── GestionComercial.Tests.csproj
```

**Componentes:**
- xUnit (framework de testing)
- Moq (mocking)
- Fixtures (setup de tests)

---

## Tecnologías Utilizadas

```
┌─────────────────────────────────────────┐
│ STACK TECNOLÓGICO                       │
│                                         │
│ Runtime:        .NET 8.0                │
│ Language:       C# 12                   │
│ UI Framework:   WPF (Windows)           │
│ MVVM Toolkit:   CommunityToolkit.MVVM   │
│ ORM:            Entity Framework Core 8 │
│ Database:       SQL Server Express      │
│ Mapping:        Mapster                 │
│ Testing:        xUnit + Moq             │
│ Logging:        Serilog                 │
│ DI Container:   Microsoft Extensions    │
└─────────────────────────────────────────┘
```

---

## Dependencias del Proyecto

```
         ┌──────────────────┐
         │ Presentation     │
         │ (WPF)            │
         └─────────┬────────┘
                   │
        ┌──────────┴──────────┐
        ▼                      ▼
┌─────────────────┐  ┌──────────────────┐
│ Application     │  │  Infrastructure  │
│ (Services)      │  │  (EF, Repos)     │
└────────┬────────┘  └────────┬─────────┘
         │                    │
         └──────────┬─────────┘
                    ▼
         ┌──────────────────┐
         │ Domain           │
         │ (Entities)       │
         └──────────────────┘
```

---

## Flujo de Compilación

```
GestionComercial.sln
├── GestionComercial.Domain (compila primero - sin dependencias)
├── GestionComercial.Application (requiere Domain)
├── GestionComercial.Infrastructure (requiere Domain + Application)
├── GestionComercial.Presentation (requiere todos - punto de entrada)
└── GestionComercial.Tests (requiere todos para testing)
```

---

## Próximos Pasos

### Fase 1: Configuración de Base de Datos
- [ ] Crear DbContext en Infrastructure
- [ ] Implementar Entity Configurations
- [ ] Generar migraciones de EF Core
- [ ] Validar contexto con BD

### Fase 2: Implementar Repositories
- [ ] Repository genérico base
- [ ] RepositoriosEspecíficos (Usuario, Cliente, Venta, etc.)
- [ ] Tests unitarios de repositories

### Fase 3: Servicios de Aplicación
- [ ] UsuariosService (login, creación, etc.)
- [ ] ClientesService (CRUD de clientes)
- [ ] VentasService (crear venta, calcular, etc.)
- [ ] InventarioService (stock, movimientos)
- [ ] PesajeService (lectura de báscula)

### Fase 4: UI en WPF
- [ ] LoginWindow + ViewModel
- [ ] MainWindow (shell principal)
- [ ] Dashboard
- [ ] Módulo de Ventas
- [ ] Módulo de Clientes
- [ ] Módulo de Inventario

### Fase 5: Integración RS232
- [ ] SerialPort adapter
- [ ] PesajeReader (lectura de báscula)
- [ ] WeightValidator
- [ ] Tests de comunicación

### Fase 6: Reportes y Auditoría
- [ ] Sistema de reportes
- [ ] Dashboard ejecutivo
- [ ] Exportación de datos

### Fase 7: Testing Completo
- [ ] Unit tests
- [ ] Integration tests
- [ ] E2E tests

---

## Cómo Compilar y Ejecutar

### Requisitos
```
- Visual Studio 2022 (versión 17.8+)
- .NET 8 SDK
- SQL Server Express 2019 o superior
- Base de datos GestionComercial_BD creada
```

### Compilar
```bash
cd "c:\Users\super\Documents\Proyectos full stack\Aplicaciones Escritorio"
dotnet build GestionComercial.sln -c Release
```

### Ejecutar
```bash
dotnet run --project GestionComercial.Presentation/GestionComercial.Presentation.csproj
```

### Ejecutar Tests
```bash
dotnet test GestionComercial.sln
```

---

## Convenciones de Código

### Naming
```
Classes:        PascalCase      (Usuario, ClientesService)
Methods:        PascalCase      (ObtenerPorId, CrearNueva)
Properties:     PascalCase      (NombreUsuario)
Private fields: _camelCase      (_usuarioRepository)
Constants:      UPPER_CASE      (MAX_INTENTOS, TIMEOUT)
```

### Estructura de Clase
```csharp
public class MiClase
{
    // 1. Campos privados
    private readonly IService _service;
    
    // 2. Propiedades públicas
    public string Nombre { get; set; }
    
    // 3. Constructores
    public MiClase(IService service) { }
    
    // 4. Métodos públ publicsos
    public void MiMetodo() { }
    
    // 5. Métodos privados
    private void MetodoPrivado() { }
}
```

---

## Contacto y Soporte

Para consultas o problemas con la estructura del proyecto:
1. Revisar comentarios XML en el código
2. Consultar documento de análisis funcional
3. Revisar tests para ejemplos de uso
