namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Venta
/// </summary>
public class VentaRepository : Repository<Venta>
{
    public VentaRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene venta por número
    /// </summary>
    public async Task<Venta?> ObtenerPorNumeroAsync(string numeroVenta)
    {
        return await _dbSet
            .Include(v => v.Cliente)
            .Include(v => v.PesajeTara)
            .Include(v => v.PesajeBruto)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
            .ThenInclude(dv => dv.Producto)
            .FirstOrDefaultAsync(v => v.NumeroVenta == numeroVenta);
    }

    /// <summary>
    /// Obtiene ventas por cliente
    /// </summary>
    public async Task<IEnumerable<Venta>> ObtenerPorClienteAsync(int idCliente, int? limitUltimas = null)
    {
        IQueryable<Venta> query = _dbSet
            .Where(v => v.ID_Cliente == idCliente)
            .Include(v => v.Detalles)
            .ThenInclude(dv => dv.Producto)
            .Include(v => v.PesajeTara)
            .Include(v => v.PesajeBruto)
            .OrderByDescending(v => v.FechaVenta);

        if (limitUltimas.HasValue)
            query = query.Take(limitUltimas.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene ventas por rango de fechas
    /// </summary>
    public async Task<IEnumerable<Venta>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta, int? idCliente = null)
    {
        IQueryable<Venta> query = _dbSet
            .Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta)
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .ThenInclude(dv => dv.Producto)
            .OrderByDescending(v => v.FechaVenta);

        if (idCliente.HasValue)
            query = query.Where(v => v.ID_Cliente == idCliente);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene ventas por tipo de documento
    /// </summary>
    public async Task<IEnumerable<Venta>> ObtenerPorTipoDocumentoAsync(string tipoDocumento)
    {
        return await _dbSet
            .Where(v => v.TipoDocumento == tipoDocumento)
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene ventas pendientes de facturación
    /// </summary>
    public async Task<IEnumerable<Venta>> ObtenerPendientesFacturacionAsync()
    {
        return await _dbSet
            .Where(v => v.EstadoVenta == "BORRADOR")
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .ThenInclude(dv => dv.Producto)
            .OrderBy(v => v.FechaVenta)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe el número de venta
    /// </summary>
    public async Task<bool> ExisteNumeroAsync(string numeroVenta)
    {
        return await _dbSet.AnyAsync(v => v.NumeroVenta == numeroVenta);
    }

    /// <summary>
    /// Obtiene total vendido en período
    /// </summary>
    public async Task<decimal> ObtenerTotalVendidoAsync(DateTime desde, DateTime hasta, int? idCliente = null)
    {
        var query = _dbSet
            .Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta);

        if (idCliente.HasValue)
            query = query.Where(v => v.ID_Cliente == idCliente);

        return await query.SumAsync(v => v.TotalVenta);
    }

    /// <summary>
    /// Obtiene promedio de ventas diarias
    /// </summary>
    public async Task<decimal> ObtenerPromedioVentasDiariasAsync(int? idCliente = null)
    {
        var query = _dbSet
            .Where(v => v.EstadoVenta != "ANULADA");

        if (idCliente.HasValue)
            query = query.Where(v => v.ID_Cliente == idCliente);

        var ventasAgrupadas = await query
            .GroupBy(v => v.FechaVenta.Date)
            .Select(g => g.Sum(v => v.TotalVenta))
            .ToListAsync();

        return ventasAgrupadas.Any() ? ventasAgrupadas.Average() : 0m;
    }

    /// <summary>
    /// Obtiene venta con todas sus relaciones
    /// </summary>
    public async Task<Venta?> ObtenerConHistorialCompleto(int idVenta)
    {
        return await _dbSet
            .Include(v => v.Cliente)
            .ThenInclude(c => c.Vehiculos)
            .Include(v => v.PesajeTara)
            .Include(v => v.PesajeBruto)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
            .ThenInclude(dv => dv.Producto)
            .FirstOrDefaultAsync(v => v.ID_Venta == idVenta);
    }
}
