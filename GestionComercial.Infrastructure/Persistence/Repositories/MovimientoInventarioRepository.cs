namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Movimiento Inventario (Auditoría de stock)
/// </summary>
public class MovimientoInventarioRepository : Repository<MovimientoInventario>
{
    public MovimientoInventarioRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene movimientos por producto
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerPorProductoAsync(int idProducto, int? limitUltimos = null)
    {
        IQueryable<MovimientoInventario> query = _dbSet
            .Where(m => m.ID_Producto == idProducto)
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMovimiento);

        if (limitUltimos.HasValue)
            query = query.Take(limitUltimos.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene movimientos por rango de fechas
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta, int? idProducto = null)
    {
        IQueryable<MovimientoInventario> query = _dbSet
            .Where(m => m.FechaMovimiento >= desde && m.FechaMovimiento <= hasta)
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMovimiento);

        if (idProducto.HasValue)
            query = query.Where(m => m.ID_Producto == idProducto);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene movimientos por tipo
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerPorTipoAsync(string tipo)
    {
        return await _dbSet
            .Where(m => m.TipoMovimiento == tipo)
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMovimiento)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene entradas de inventario
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerEntradasAsync(int? idProducto = null)
    {
        IQueryable<MovimientoInventario> query = _dbSet
            .Where(m => m.TipoMovimiento == "ENTRADA")
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMovimiento);

        if (idProducto.HasValue)
            query = query.Where(m => m.ID_Producto == idProducto);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene salidas de inventario
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerSalidasAsync(int? idProducto = null)
    {
        IQueryable<MovimientoInventario> query = _dbSet
            .Where(m => m.TipoMovimiento == "SALIDA")
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMovimiento);

        if (idProducto.HasValue)
            query = query.Where(m => m.ID_Producto == idProducto);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene ajustes de inventario
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerAjustesAsync()
    {
        return await _dbSet
            .Where(m => m.TipoMovimiento == "AJUSTE")
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMovimiento)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene movimientos por usuario
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerPorUsuarioAsync(int idUsuario, DateTime? desde = null, DateTime? hasta = null)
    {
        IQueryable<MovimientoInventario> query = _dbSet
            .Where(m => m.UsuarioMovimiento == idUsuario)
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMovimiento);

        if (desde.HasValue)
            query = query.Where(m => m.FechaMovimiento >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(m => m.FechaMovimiento <= hasta.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Calcula la cantidad movilizada en período
    /// </summary>
    public async Task<decimal> ObtenerCantidadMovilizadaAsync(int idProducto, DateTime desde, DateTime hasta, string? tipo = null)
    {
        IQueryable<MovimientoInventario> query = _dbSet
            .Where(m => m.ID_Producto == idProducto &&
                        m.FechaMovimiento >= desde &&
                        m.FechaMovimiento <= hasta);

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(m => m.TipoMovimiento == tipo);

        return await query.SumAsync(m => m.Cantidad);
    }

    /// <summary>
    /// Obtiene movimiento de balance completo por producto
    /// </summary>
    public async Task<(decimal Entradas, decimal Salidas, decimal Neto)> ObtenerBalanceProductoAsync(int idProducto)
    {
        var movimientos = await _dbSet
            .Where(m => m.ID_Producto == idProducto)
            .ToListAsync();

        var entradas = movimientos.Where(m => m.TipoMovimiento == "ENTRADA").Sum(m => m.Cantidad);
        var salidas = movimientos.Where(m => m.TipoMovimiento == "SALIDA").Sum(m => m.Cantidad);
        var neto = entradas - salidas;

        return (entradas, salidas, neto);
    }

    /// <summary>
    /// Obtiene discrepancias de inventario (movimientos anómalos para auditoría)
    /// </summary>
    public async Task<IEnumerable<MovimientoInventario>> ObtenerDiscrepanciasAsync(decimal umbralAnomaliaUnidades = 1000)
    {
        return await _dbSet
            .Where(m => m.Cantidad > umbralAnomaliaUnidades || m.Cantidad < -umbralAnomaliaUnidades)
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaMovimiento)
            .ToListAsync();
    }
}
