namespace GestionComercial.Infrastructure.Persistence.Migrations;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// Migración inicial para crear la estructura de base de datos
/// Generada por: dotnet ef migrations add InitialCreate
/// Se debe ejecutar: dotnet ef database update
/// </summary>
[DbContext(typeof(GestionComercialContext))]
[Migration("20260417000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Esta migración es generada automáticamente por EF Core
        // Se crearán todas las tablas basadas en el DbContext
        // El script SQL completo está en SCRIPT_BASE_DATOS.sql
        
        // Para aplicar esta migración, ejecutar:
        // dotnet ef database update --project GestionComercial.Infrastructure
        
        // Las tablas se crearán automáticamente según las configuraciones en DbContext
        // - Roles
        // - Usuarios
        // - Clientes
        // - Vehiculos
        // - Productos
        // - Pesajes
        // - Ventas
        // - Detalle_Ventas
        // - Descuentos
        // - Facturas
        // - Movimientos_Inventario
        // - Lotes_Inventario
        // - Auditoria
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Para revertir esta migración:
        // dotnet ef migrations remove
        
        // Todas las tablas serán eliminadas
    }
}
