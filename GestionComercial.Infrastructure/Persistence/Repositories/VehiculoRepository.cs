namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Vehículo
/// </summary>
public class VehiculoRepository : Repository<Vehiculo>, IVehiculoRepository
{
    public VehiculoRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene vehículo por placa
    /// </summary>
    public async Task<Vehiculo?> ObtenerPorPlacaAsync(string placa)
    {
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.Pesajes)
            .FirstOrDefaultAsync(v => v.Placa == placa);
    }

    /// <summary>
    /// Obtiene vehículos de un cliente
    /// </summary>
    public async Task<IEnumerable<Vehiculo>> ObtenerPorClienteAsync(int idCliente)
    {
        return await _dbSet
            .Where(v => v.ID_Cliente == idCliente)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene vehículos activos de un cliente
    /// </summary>
    public async Task<IEnumerable<Vehiculo>> ObtenerActivosPorClienteAsync(int idCliente)
    {
        return await _dbSet
            .Where(v => v.ID_Cliente == idCliente && v.Estado == "ACTIVO")
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene todos los vehículos activos
    /// </summary>
    public async Task<IEnumerable<Vehiculo>> ObtenerActivosAsync()
    {
        return await _dbSet
            .Where(v => v.Estado == "ACTIVO")
            .Include(v => v.Cliente)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe la placa
    /// </summary>
    public async Task<bool> ExistePlacaAsync(string placa)
    {
        return await _dbSet.AnyAsync(v => v.Placa == placa);
    }

    /// <summary>
    /// Obtiene vehículos que necesitan mantenimiento
    /// </summary>
    public async Task<IEnumerable<Vehiculo>> ObtenerEnMantenimientoAsync()
    {
        return await _dbSet
            .Where(v => v.Estado == "MANTENIMIENTO")
            .Include(v => v.Cliente)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene último pesaje de un vehículo
    /// </summary>
    public async Task<Pesaje?> ObtenerUltimoPesajeAsync(int idVehiculo)
    {
        return await _context.Pesajes
            .Where(p => p.ID_Vehiculo == idVehiculo)
            .OrderByDescending(p => p.FechaPesaje)
            .FirstOrDefaultAsync();
    }
}
