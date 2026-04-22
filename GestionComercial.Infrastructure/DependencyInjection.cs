namespace GestionComercial.Infrastructure;

using GestionComercial.Application.Interfaces;
using GestionComercial.Application.Services;
using GestionComercial.Infrastructure.Persistence;
using GestionComercial.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensión de servicios para inyección de dependencias de Infrastructure
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra servicios de infraestructura en el contenedor de DI
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <returns>Colección de servicios para encadenamiento</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // =====================================================
        // Registrar DbContext
        // =====================================================
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada en appsettings.json");

        services.AddDbContext<GestionComercialContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName);
                sqlOptions.CommandTimeout(30);  // Timeout de 30 segundos
            }));

        // =====================================================
        // Registrar Repositorio Genérico
        // =====================================================
        services
            .AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // =====================================================
        // Registrar Repositorios Especializados
        // =====================================================
        
        // Usuario Repository
        services
            .AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Cliente Repository
        services
            .AddScoped<IClienteRepository, ClienteRepository>();

        // Vehículo Repository
        services
            .AddScoped<IVehiculoRepository, VehiculoRepository>();

        // Producto Repository
        services
            .AddScoped<IRepository<GestionComercial.Domain.Entities.Producto>, ProductoRepository>();

        // Venta Repository
        services
            .AddScoped<IRepository<GestionComercial.Domain.Entities.Venta>, VentaRepository>();

        // Pesaje Repository
        services
            .AddScoped<IRepository<GestionComercial.Domain.Entities.Pesaje>, PesajeRepository>();

        // Factura Repository
        services
            .AddScoped<IRepository<GestionComercial.Domain.Entities.Factura>, FacturaRepository>();

        // Movimiento Inventario Repository
        services
            .AddScoped<IRepository<GestionComercial.Domain.Entities.MovimientoInventario>, MovimientoInventarioRepository>();

        // Lote Inventario Repository
        services
            .AddScoped<IRepository<GestionComercial.Domain.Entities.LoteInventario>, LoteInventarioRepository>();

        // Auditoría Repository
        services
            .AddScoped<IRepository<GestionComercial.Domain.Entities.Auditoria>, AuditoriaRepository>();

        // =====================================================
        // Registrar Servicios de Aplicación
        // =====================================================
        
        // Usuario Service
        services
            .AddScoped<IUsuarioService, UsuarioService>();

        // Cliente Service
        services
            .AddScoped<IClienteService, ClienteService>();

        // Venta Service
        services
            .AddScoped<IVentaService, VentaService>();

        services
            .AddScoped<IInventarioService, InventarioService>();

        services
            .AddScoped<IPesajeService, PesajeService>();

        services
            .AddScoped<IFacturaService, FacturaService>();

        services
            .AddScoped<IVehiculoService, VehiculoService>();

        return services;
    }
}
