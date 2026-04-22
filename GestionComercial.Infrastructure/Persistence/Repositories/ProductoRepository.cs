namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Producto
/// </summary>
public class ProductoRepository : Repository<Producto>
{
    public ProductoRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene producto por código
    /// </summary>
    public async Task<Producto?> ObtenerPorCodigoAsync(string codigo)
    {
        return await _dbSet
            .Include(p => p.Lotes)
            .FirstOrDefaultAsync(p => p.Codigo == codigo);
    }

    /// <summary>
    /// Obtiene productos activos
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerActivosAsync()
    {
        return await _dbSet
            .Where(p => p.Estado == "ACTIVO")
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene productos por tipo de material
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerPorTipoAsync(string tipoMaterial)
    {
        return await _dbSet
            .Where(p => p.TipoMaterial == tipoMaterial && p.Estado == "ACTIVO")
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene productos con stock bajo
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerStockBajoAsync()
    {
        return await _dbSet
            .Where(p => p.Stock < p.StockMinimo && p.Estado == "ACTIVO")
            .OrderBy(p => p.Stock)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene productos más vendidos
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerMasVendidosAsync(int top = 10, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.DetallesVentas
            .AsQueryable();

        if (desde.HasValue)
            query = query.Where(dv => dv.Venta.FechaVenta >= desde.Value);
        
        if (hasta.HasValue)
            query = query.Where(dv => dv.Venta.FechaVenta <= hasta.Value);

        var productosVendidos = await query
            .GroupBy(dv => dv.ID_Producto)
            .OrderByDescending(g => g.Sum(dv => dv.Cantidad))
            .Take(top)
            .Select(g => g.Key)
            .ToListAsync();

        return await _dbSet
            .Where(p => productosVendidos.Contains(p.ID_Producto))
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe el código
    /// </summary>
    public async Task<bool> ExisteCodigoAsync(string codigo)
    {
        return await _dbSet.AnyAsync(p => p.Codigo == codigo);
    }

    /// <summary>
    /// Obtiene valor total del inventario
    /// </summary>
    public async Task<decimal> ObtenerValorInventarioTotalAsync()
    {
        return await _dbSet
            .Where(p => p.Estado == "ACTIVO")
            .SumAsync(p => p.Stock * p.PrecioBase);
    }

    /// <summary>
    /// Actualiza el stock de un producto
    /// </summary>
    public async Task ActualizarStockAsync(int idProducto, decimal cantidad)
    {
        var producto = await _dbSet.FindAsync(idProducto);
        if (producto != null)
        {
            producto.Stock += cantidad;
            producto.FechaModificacion = DateTime.Now;
            await ActualizarAsync(producto);
        }
    }
}
