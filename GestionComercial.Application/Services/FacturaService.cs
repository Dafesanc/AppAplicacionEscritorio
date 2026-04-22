namespace GestionComercial.Application.Services;

using GestionComercial.Application.DTOs;
using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.Extensions.Logging;

public class FacturaService : IFacturaService
{
    private readonly IRepository<Factura> _facturaRepository;
    private readonly IRepository<Venta> _ventaRepository;
    private readonly ILogger<FacturaService> _logger;

    public FacturaService(
        IRepository<Factura> facturaRepository,
        IRepository<Venta> ventaRepository,
        ILogger<FacturaService> logger)
    {
        _facturaRepository = facturaRepository;
        _ventaRepository = ventaRepository;
        _logger = logger;
    }

    public async Task<FacturaDTO?> ObtenerPorIdAsync(int idFactura)
    {
        try
        {
            var factura = await _facturaRepository.ObtenerPorIdAsync(idFactura);
            return factura == null ? null : MapearDTO(factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorIdAsync");
            return null;
        }
    }

    public async Task<FacturaDTO?> ObtenerPorNumeroAsync(string numeroFactura)
    {
        try
        {
            var facturas = await _facturaRepository.ObtenerTodosAsync();
            var factura = facturas.FirstOrDefault(f => f.NumeroFactura == numeroFactura);
            return factura == null ? null : MapearDTO(factura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorNumeroAsync");
            return null;
        }
    }

    public async Task<IEnumerable<FacturaDTO>> ObtenerPendientesPagoAsync(int? idCliente = null)
    {
        try
        {
            var facturas = await _facturaRepository.ObtenerTodosAsync();
            var query = facturas.Where(f => f.EstadoFactura == "EMITIDA" || f.EstadoFactura == "CRÉDITO");

            if (idCliente.HasValue)
                query = query.Where(f => f.ID_Cliente == idCliente.Value);

            return query.OrderBy(f => f.FechaEmision).Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPendientesPagoAsync");
            return Enumerable.Empty<FacturaDTO>();
        }
    }

    public async Task<IEnumerable<FacturaDTO>> ObtenerVencidasAsync(int diasVencimiento = 30)
    {
        try
        {
            var fechaLimite = DateTime.Now.AddDays(-diasVencimiento);
            var facturas = await _facturaRepository.ObtenerTodosAsync();
            return facturas
                .Where(f => (f.EstadoFactura == "EMITIDA" || f.EstadoFactura == "CRÉDITO")
                             && f.FechaEmision <= fechaLimite)
                .OrderBy(f => f.FechaEmision)
                .Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerVencidasAsync");
            return Enumerable.Empty<FacturaDTO>();
        }
    }

    public async Task<int?> GenerarFacturaAsync(int idVenta)
    {
        try
        {
            var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta);
            if (venta == null) return null;

            var factura = new Factura
            {
                NumeroFactura = $"FAC-{DateTime.Now:yyyyMMddHHmmss}",
                ID_Venta = idVenta,
                ID_Cliente = venta.ID_Cliente,
                FechaEmision = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddDays(30),
                SubtotalFactura = venta.Subtotal,
                IVAFactura = venta.IVA,
                TotalFactura = venta.TotalVenta,
                EstadoFactura = "EMITIDA",
                UsuarioEmision = 0
            };

            await _facturaRepository.AgregarAsync(factura);
            _logger.LogInformation("Factura generada: {Numero}", factura.NumeroFactura);
            return factura.ID_Factura;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GenerarFacturaAsync");
            return null;
        }
    }

    public async Task<bool> MarcarPagadaAsync(int idFactura, decimal montoRecibido)
    {
        try
        {
            var factura = await _facturaRepository.ObtenerPorIdAsync(idFactura);
            if (factura == null) return false;

            factura.EstadoFactura = "PAGADA";
            await _facturaRepository.ActualizarAsync(factura);

            _logger.LogInformation("Factura marcada como pagada: {Id}, monto {Monto}", idFactura, montoRecibido);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en MarcarPagadaAsync");
            return false;
        }
    }

    public async Task<decimal> ObtenerTotalPendientePagoAsync(int? idCliente = null)
    {
        try
        {
            var facturas = await _facturaRepository.ObtenerTodosAsync();
            var query = facturas.Where(f => f.EstadoFactura == "EMITIDA" || f.EstadoFactura == "CRÉDITO");

            if (idCliente.HasValue)
                query = query.Where(f => f.ID_Cliente == idCliente.Value);

            return query.Sum(f => f.TotalFactura ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerTotalPendientePagoAsync");
            return 0;
        }
    }

    private static FacturaDTO MapearDTO(Factura f) => new()
    {
        IdFactura = f.ID_Factura,
        NumeroFactura = f.NumeroFactura,
        IdVenta = f.ID_Venta,
        NumeroVenta = f.Venta?.NumeroVenta ?? string.Empty,
        IdCliente = f.ID_Cliente,
        ClienteNombre = f.Cliente?.Nombre ?? f.Venta?.Cliente?.Nombre ?? string.Empty,
        FechaEmision = f.FechaEmision,
        FechaVencimiento = f.FechaVencimiento,
        SubtotalFactura = f.SubtotalFactura ?? 0,
        IVAFactura = f.IVAFactura ?? 0,
        TotalFactura = f.TotalFactura ?? 0,
        MontoPagado = f.EstadoFactura == "PAGADA" ? f.TotalFactura ?? 0 : 0,
        Estado = f.EstadoFactura
    };
}
