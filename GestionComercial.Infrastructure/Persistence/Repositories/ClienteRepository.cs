namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Cliente con métodos especializados
/// </summary>
public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene cliente por número de identificación
    /// </summary>
    public async Task<Cliente?> ObtenerPorIdentificacionAsync(string numeroIdentificacion)
    {
        return await _dbSet
            .Include(c => c.Vehiculos)
            .FirstOrDefaultAsync(c => c.NumeroIdentificacion == numeroIdentificacion);
    }

    /// <summary>
    /// Obtiene cliente por código
    /// </summary>
    public async Task<Cliente?> ObtenerPorCodigoAsync(string codigo)
    {
        return await _dbSet
            .Include(c => c.Vehiculos)
            .FirstOrDefaultAsync(c => c.CodigoCliente == codigo);
    }

    /// <summary>
    /// Obtiene todos los clientes activos
    /// </summary>
    public async Task<IEnumerable<Cliente>> ObtenerActivosAsync()
    {
        return await _dbSet
            .Where(c => c.Estado == "ACTIVO")
            .Include(c => c.Vehiculos)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene clientes por categoría
    /// </summary>
    public async Task<IEnumerable<Cliente>> ObtenerPorCategoriaAsync(string categoria)
    {
        return await _dbSet
            .Where(c => c.Categoria == categoria && c.Estado == "ACTIVO")
            .Include(c => c.Vehiculos)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene clientes frecuentes (top por número de transacciones)
    /// </summary>
    public async Task<IEnumerable<Cliente>> ObtenerClientesFrecuentesAsync(int top = 10, int? limitMeses = null)
    {
        IQueryable<Cliente> query = _dbSet.Include(c => c.Ventas);

        if (limitMeses.HasValue)
        {
            var fechaLimite = DateTime.Now.AddMonths(-limitMeses.Value);
            query = query.Where(c => c.Ventas.Any(v => v.FechaVenta >= fechaLimite));
        }

        return await query
            .OrderByDescending(c => c.Ventas.Count)
            .Take(top)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene clientes con crédito disponible
    /// </summary>
    public async Task<IEnumerable<Cliente>> ObtenerConCreditoDisponibleAsync()
    {
        return await _dbSet
            .Where(c => c.LimiteCredito > c.SaldoCredito && c.Estado == "ACTIVO")
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe el RUC/Identificación
    /// </summary>
    public async Task<bool> ExisteIdentificacionAsync(string identificacion)
    {
        return await _dbSet.AnyAsync(c => c.NumeroIdentificacion == identificacion);
    }

    /// <summary>
    /// Obtiene cliente con su historial completo
    /// </summary>
    public async Task<Cliente?> ObtenerConHistorialAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Vehiculos)
            .Include(c => c.Ventas)
            .FirstOrDefaultAsync(c => c.ID_Cliente == id);
    }
}
