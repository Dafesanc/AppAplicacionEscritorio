namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para Lote Inventario (Batch tracking con FIFO)
/// </summary>
public class LoteInventarioRepository : Repository<LoteInventario>
{
    public LoteInventarioRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene lotes activos de un producto
    /// </summary>
    public async Task<IEnumerable<LoteInventario>> ObtenerLotesActivosAsync(int idProducto)
    {
        return await _dbSet
            .Where(l => l.ID_Producto == idProducto && l.QuantidadDisponible > 0)
            .Include(l => l.Producto)
            .OrderBy(l => l.FechaFabricacion)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene lotes por número
    /// </summary>
    public async Task<LoteInventario?> ObtenerPorNumeroAsync(string numeroLote)
    {
        return await _dbSet
            .Include(l => l.Producto)
            .FirstOrDefaultAsync(l => l.NumeroLote == numeroLote);
    }

    /// <summary>
    /// Obtiene lotes de un producto
    /// </summary>
    public async Task<IEnumerable<LoteInventario>> ObtenerLotesProductoAsync(int idProducto)
    {
        return await _dbSet
            .Where(l => l.ID_Producto == idProducto)
            .Include(l => l.Producto)
            .OrderBy(l => l.FechaFabricacion)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene lotes próximos a expirar
    /// </summary>
    public async Task<IEnumerable<LoteInventario>> ObtenerProximosAExpirarAsync(int diasAnticipacion = 30)
    {
        var fechaLimite = DateTime.Now.AddDays(diasAnticipacion);
        return await _dbSet
            .Where(l => l.FechaVencimiento <= fechaLimite && l.FechaVencimiento > DateTime.Now && l.QuantidadDisponible > 0)
            .Include(l => l.Producto)
            .OrderBy(l => l.FechaVencimiento)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene lotes expirados
    /// </summary>
    public async Task<IEnumerable<LoteInventario>> ObtenerExpiradosAsync()
    {
        return await _dbSet
            .Where(l => l.FechaVencimiento <= DateTime.Now && l.QuantidadDisponible > 0)
            .Include(l => l.Producto)
            .OrderBy(l => l.FechaVencimiento)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe el número de lote
    /// </summary>
    public async Task<bool> ExisteNumeroAsync(string numeroLote)
    {
        return await _dbSet.AnyAsync(l => l.NumeroLote == numeroLote);
    }

    /// <summary>
    /// Obtiene próximo lote a consumir (FIFO) para un producto
    /// </summary>
    public async Task<LoteInventario?> ObtenerProximoLoteAsync(int idProducto)
    {
        return await _dbSet
            .Where(l => l.ID_Producto == idProducto && l.QuantidadDisponible > 0)
            .OrderBy(l => l.FechaFabricacion)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Calcula cantidad total disponible de un producto
    /// </summary>
    public async Task<decimal> ObtenerCantidadDisponibleAsync(int idProducto)
    {
        return await _dbSet
            .Where(l => l.ID_Producto == idProducto && l.QuantidadDisponible > 0)
            .SumAsync(l => l.QuantidadDisponible);
    }

    /// <summary>
    /// Decrementa cantidad disponible en lote
    /// </summary>
    public async Task DecrementarCantidadAsync(int idLote, decimal cantidad)
    {
        var lote = await _dbSet.FindAsync(idLote);
        if (lote != null)
        {
            lote.QuantidadDisponible -= cantidad;
            if (lote.QuantidadDisponible < 0)
                lote.QuantidadDisponible = 0;

            await ActualizarAsync(lote);
        }
    }

    /// <summary>
    /// Obtiene utilización porcentual de lote
    /// </summary>
    public async Task<decimal> ObtenerUtilizacionPorcentajeAsync(int idLote)
    {
        var lote = await _dbSet.FindAsync(idLote);
        if (lote == null || lote.QuantidadRecibida == 0)
            return 0m;

        var utilizado = lote.QuantidadRecibida - lote.QuantidadDisponible;
        return (utilizado / lote.QuantidadRecibida) * 100;
    }

    /// <summary>
    /// Obtiene lotes por rango de fechas de ingreso
    /// </summary>
    public async Task<IEnumerable<LoteInventario>> ObtenerPorPeriodoIngresoAsync(DateTime desde, DateTime hasta)
    {
        return await _dbSet
            .Where(l => l.FechaIngreso >= desde && l.FechaIngreso <= hasta)
            .Include(l => l.Producto)
            .OrderBy(l => l.FechaIngreso)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene promedio de vida útil de lote en días
    /// </summary>
    public async Task<int> ObtenerPromedioVidaUtilAsync(int idProducto)
    {
        var lotes = await _dbSet
            .Where(l => l.ID_Producto == idProducto && l.FechaVencimiento > DateTime.Now)
            .ToListAsync();

        if (lotes.Count == 0)
            return 0;

        var diasVida = lotes
            .Where(l => l.FechaVencimiento.HasValue && l.FechaFabricacion.HasValue)
            .Select(l => (l.FechaVencimiento!.Value - l.FechaFabricacion!.Value).TotalDays)
            .DefaultIfEmpty(0)
            .Average();

        return (int)diasVida;
    }
}
