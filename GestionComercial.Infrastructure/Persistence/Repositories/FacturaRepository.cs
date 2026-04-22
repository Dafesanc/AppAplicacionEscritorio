namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Factura
/// </summary>
public class FacturaRepository : Repository<Factura>
{
    public FacturaRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene factura por número
    /// </summary>
    public async Task<Factura?> ObtenerPorNumeroAsync(string numeroFactura)
    {
        return await _dbSet
            .Include(f => f.Venta)
            .ThenInclude(v => v.Cliente)
            .Include(f => f.Venta)
            .ThenInclude(v => v.Detalles)
            .ThenInclude(dv => dv.Producto)
            .FirstOrDefaultAsync(f => f.NumeroFactura == numeroFactura);
    }

    /// <summary>
    /// Obtiene facturas por cliente
    /// </summary>
    public async Task<IEnumerable<Factura>> ObtenerPorClienteAsync(int idCliente, int? limitUltimas = null)
    {
        IQueryable<Factura> query = _dbSet
            .Where(f => f.Venta.ID_Cliente == idCliente)
            .Include(f => f.Venta)
            .ThenInclude(v => v.Detalles)
            .OrderByDescending(f => f.FechaEmision);

        if (limitUltimas.HasValue)
            query = query.Take(limitUltimas.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene facturas por rango de fechas
    /// </summary>
    public async Task<IEnumerable<Factura>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta, int? idCliente = null)
    {
        IQueryable<Factura> query = _dbSet
            .Where(f => f.FechaEmision >= desde && f.FechaEmision <= hasta)
            .Include(f => f.Venta)
            .ThenInclude(v => v.Cliente)
            .OrderByDescending(f => f.FechaEmision);

        if (idCliente.HasValue)
            query = query.Where(f => f.Venta.ID_Cliente == idCliente);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene facturas por estado
    /// </summary>
    public async Task<IEnumerable<Factura>> ObtenerPorEstadoAsync(string estado)
    {
        return await _dbSet
            .Where(f => f.EstadoFactura == estado)
            .Include(f => f.Venta)
            .ThenInclude(v => v.Cliente)
            .OrderBy(f => f.FechaEmision)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene facturas pendientes de pago
    /// </summary>
    public async Task<IEnumerable<Factura>> ObtenerPendientesPagoAsync(int? idCliente = null)
    {
        IQueryable<Factura> query = _dbSet
            .Where(f => f.EstadoFactura == "EMITIDA" || f.EstadoFactura == "CRÉDITO")
            .Include(f => f.Venta)
            .ThenInclude(v => v.Cliente)
            .OrderBy(f => f.FechaEmision);

        if (idCliente.HasValue)
            query = query.Where(f => f.Venta.ID_Cliente == idCliente);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene facturas vencidas (sin pagar después de N días)
    /// </summary>
    public async Task<IEnumerable<Factura>> ObtenerVencidasAsync(int diasVencimiento = 30)
    {
        var fechaVencimiento = DateTime.Now.AddDays(-diasVencimiento);
        return await _dbSet
            .Where(f => (f.EstadoFactura == "EMITIDA" || f.EstadoFactura == "CRÉDITO") && f.FechaEmision <= fechaVencimiento)
            .Include(f => f.Venta)
            .ThenInclude(v => v.Cliente)
            .OrderBy(f => f.FechaEmision)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe el número de factura
    /// </summary>
    public async Task<bool> ExisteNumeroAsync(string numeroFactura)
    {
        return await _dbSet.AnyAsync(f => f.NumeroFactura == numeroFactura);
    }

    /// <summary>
    /// Obtiene total facturado en período
    /// </summary>
    public async Task<decimal> ObtenerTotalFacturadoAsync(DateTime desde, DateTime hasta, int? idCliente = null)
    {
        IQueryable<Factura> query = _dbSet
            .Where(f => f.FechaEmision >= desde && f.FechaEmision <= hasta);

        if (idCliente.HasValue)
            query = query.Where(f => f.Venta.ID_Cliente == idCliente);

        return await query.SumAsync(f => f.TotalFactura ?? 0);
    }

    /// <summary>
    /// Obtiene total pendiente de pago (suma de facturas no pagadas)
    /// </summary>
    public async Task<decimal> ObtenerTotalPendientePagoAsync(int? idCliente = null)
    {
        IQueryable<Factura> query = _dbSet
            .Where(f => f.EstadoFactura == "EMITIDA" || f.EstadoFactura == "CRÉDITO");

        if (idCliente.HasValue)
            query = query.Where(f => f.Venta.ID_Cliente == idCliente);

        return await query.SumAsync(f => f.TotalFactura ?? 0);
    }

    /// <summary>
    /// Obtiene factura completa con todos los detalles
    /// </summary>
    public async Task<Factura?> ObtenerConDetalleCompletAsync(int idFactura)
    {
        return await _dbSet
            .Include(f => f.Venta)
            .ThenInclude(v => v.Cliente)
            .ThenInclude(c => c.Vehiculos)
            .Include(f => f.Venta)
            .ThenInclude(v => v.Detalles)
            .ThenInclude(dv => dv.Producto)
            .Include(f => f.Venta)
            .ThenInclude(v => v.PesajeTara)
            .Include(f => f.Venta)
            .ThenInclude(v => v.PesajeBruto)
            .FirstOrDefaultAsync(f => f.ID_Factura == idFactura);
    }
}
