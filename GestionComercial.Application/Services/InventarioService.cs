namespace GestionComercial.Application.Services;

using GestionComercial.Application.DTOs;
using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.Extensions.Logging;

public class InventarioService : IInventarioService
{
    private readonly IRepository<Producto> _productoRepository;
    private readonly IRepository<MovimientoInventario> _movimientoRepository;
    private readonly ILogger<InventarioService> _logger;

    public InventarioService(
        IRepository<Producto> productoRepository,
        IRepository<MovimientoInventario> movimientoRepository,
        ILogger<InventarioService> logger)
    {
        _productoRepository = productoRepository;
        _movimientoRepository = movimientoRepository;
        _logger = logger;
    }

    public async Task<ProductoDTO?> ObtenerPorIdAsync(int idProducto)
    {
        try
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(idProducto);
            return producto == null ? null : MapearDTO(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorIdAsync");
            return null;
        }
    }

    public async Task<IEnumerable<ProductoDTO>> ObtenerStockBajoAsync()
    {
        try
        {
            var productos = await _productoRepository.ObtenerTodosAsync();
            return productos
                .Where(p => p.Estado == "ACTIVO" && p.Stock < p.StockMinimo)
                .Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerStockBajoAsync");
            return Enumerable.Empty<ProductoDTO>();
        }
    }

    public async Task<IEnumerable<ProductoDTO>> ObtenerActivosAsync()
    {
        try
        {
            var productos = await _productoRepository.ObtenerTodosAsync();
            return productos.Where(p => p.Estado == "ACTIVO").Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerActivosAsync");
            return Enumerable.Empty<ProductoDTO>();
        }
    }

    public async Task<bool> ActualizarStockAsync(int idProducto, decimal cantidad, string tipo, string referencia)
    {
        try
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(idProducto);
            if (producto == null) return false;

            var stockAnterior = producto.Stock;
            producto.Stock += tipo == "ENTRADA" ? cantidad : -cantidad;
            producto.FechaModificacion = DateTime.Now;
            await _productoRepository.ActualizarAsync(producto);

            var movimiento = new MovimientoInventario
            {
                ID_Producto = idProducto,
                TipoMovimiento = tipo,
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockPosterior = producto.Stock,
                Referencia = referencia,
                FechaMovimiento = DateTime.Now,
                UsuarioMovimiento = 0
            };
            await _movimientoRepository.AgregarAsync(movimiento);

            _logger.LogInformation("Stock actualizado: producto {Id}, tipo {Tipo}, cantidad {Cant}", idProducto, tipo, cantidad);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ActualizarStockAsync");
            return false;
        }
    }

    public async Task<bool> AjustarStockAsync(int idProducto, decimal cantidadAjuste, string razon)
    {
        var tipo = cantidadAjuste >= 0 ? "AJUSTE" : "AJUSTE";
        return await ActualizarStockAsync(idProducto, Math.Abs(cantidadAjuste), tipo, razon);
    }

    public async Task<decimal> ObtenerValorInventarioTotalAsync()
    {
        try
        {
            var productos = await _productoRepository.ObtenerTodosAsync();
            return productos.Where(p => p.Estado == "ACTIVO").Sum(p => p.Stock * p.PrecioBase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerValorInventarioTotalAsync");
            return 0;
        }
    }

    public async Task<IEnumerable<MovimientoInventarioDTO>> ObtenerMovimientosAsync(int idProducto, int? diasAtras = null)
    {
        try
        {
            var movimientos = await _movimientoRepository.ObtenerTodosAsync();
            var query = movimientos.Where(m => m.ID_Producto == idProducto);

            if (diasAtras.HasValue)
            {
                var desde = DateTime.Now.AddDays(-diasAtras.Value);
                query = query.Where(m => m.FechaMovimiento >= desde);
            }

            return query.OrderByDescending(m => m.FechaMovimiento).Select(m => new MovimientoInventarioDTO
            {
                IdMovimiento = m.ID_Movimiento,
                IdProducto = m.ID_Producto,
                Tipo = m.TipoMovimiento,
                Cantidad = m.Cantidad,
                StockAnterior = m.StockAnterior ?? 0,
                StockPosterior = m.StockPosterior ?? 0,
                Referencia = m.Referencia,
                FechaMovimiento = m.FechaMovimiento
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerMovimientosAsync");
            return Enumerable.Empty<MovimientoInventarioDTO>();
        }
    }

    private static ProductoDTO MapearDTO(Producto p) => new()
    {
        IdProducto = p.ID_Producto,
        Codigo = p.Codigo,
        Nombre = p.Nombre,
        TipoMaterial = p.TipoMaterial,
        Unidad = p.Unidad,
        PrecioBase = p.PrecioBase,
        Stock = p.Stock,
        StockMinimo = p.StockMinimo,
        StockMaximo = p.StockMaximo,
        Estado = p.Estado
    };
}
