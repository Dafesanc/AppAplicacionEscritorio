namespace GestionComercial.Application.Services;

using GestionComercial.Application.DTOs;
using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.Extensions.Logging;

/// <summary>
/// Servicio para gestión de Ventas
/// </summary>
public class VentaService : IVentaService
{
    private readonly IRepository<Venta> _ventaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<VentaService> _logger;

    public VentaService(
        IRepository<Venta> ventaRepository,
        IClienteRepository clienteRepository,
        ILogger<VentaService> logger)
    {
        _ventaRepository = ventaRepository;
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    public async Task<VentaDTO?> ObtenerPorIdAsync(int idVenta)
    {
        try
        {
            var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta);
            return venta == null ? null : MapearDTO(venta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorIdAsync");
            return null;
        }
    }

    public async Task<VentaDTO?> ObtenerPorNumeroAsync(string numeroVenta)
    {
        try
        {
            var ventas = await _ventaRepository.ObtenerTodosAsync();
            var venta = ventas.FirstOrDefault(v => v.NumeroVenta == numeroVenta);
            return venta == null ? null : MapearDTO(venta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorNumeroAsync");
            return null;
        }
    }

    public async Task<IEnumerable<VentaDTO>> ObtenerPorClienteAsync(int idCliente)
    {
        try
        {
            var ventas = await _ventaRepository.ObtenerTodosAsync();
            return ventas
                .Where(v => v.ID_Cliente == idCliente)
                .OrderByDescending(v => v.FechaVenta)
                .Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorClienteAsync");
            return Enumerable.Empty<VentaDTO>();
        }
    }

    public async Task<IEnumerable<VentaDTO>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            var ventas = await _ventaRepository.ObtenerTodosAsync();
            return ventas
                .Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta)
                .OrderByDescending(v => v.FechaVenta)
                .Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorFechaAsync");
            return Enumerable.Empty<VentaDTO>();
        }
    }

    public async Task<int?> CrearAsync(CrearVentaDTO ventaDTO)
    {
        try
        {
            var venta = new Venta
            {
                NumeroVenta = $"VENTA-{DateTime.Now:yyyyMMddHHmmss}",
                ID_Cliente = ventaDTO.IdCliente,
                ID_Vehiculo = ventaDTO.IdVehiculo > 0 ? ventaDTO.IdVehiculo : null,
                ID_PesajeTara = ventaDTO.IdPesajeTara > 0 ? ventaDTO.IdPesajeTara : null,
                ID_PesajeBruto = ventaDTO.IdPesajeBruto > 0 ? ventaDTO.IdPesajeBruto : null,
                TipoDocumento = ventaDTO.TipoDocumento,
                EstadoVenta = "BORRADOR",
                UsuarioVenta = 0, // TODO: obtener del contexto de sesión del usuario autenticado
                FechaVenta = DateTime.Now,
                FechaModificacion = DateTime.Now
            };

            // TODO: Calcular Subtotal, IVA y TotalVenta desde ventaDTO.Detalles

            await _ventaRepository.AgregarAsync(venta);
            _logger.LogInformation("Venta creada: {NumeroVenta}", venta.NumeroVenta);
            return venta.ID_Venta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CrearAsync");
            return null;
        }
    }

    public async Task<int?> CrearManualAsync(CrearVentaManualDTO dto)
    {
        try
        {
            var totalFinal = Math.Max(0, dto.Total - dto.Descuento);
            var venta = new Venta
            {
                NumeroVenta      = $"VENTA-{DateTime.Now:yyyyMMddHHmmss}",
                ID_Cliente       = dto.IdCliente,
                ID_Vehiculo      = dto.IdVehiculo is > 0 ? dto.IdVehiculo : null,
                PesoNetoKg       = dto.PesoNetoKg,
                Subtotal         = dto.Total,
                DescuentosAplicados = dto.Descuento,
                IVA              = 0,
                TotalVenta       = totalFinal,
                TipoDocumento    = dto.TipoDocumento,
                EstadoVenta      = "BORRADOR",
                UsuarioVenta     = dto.UsuarioId,
                FechaVenta       = DateTime.Now,
                FechaModificacion = DateTime.Now
            };
            await _ventaRepository.AgregarAsync(venta);
            _logger.LogInformation("Venta manual creada: {NumeroVenta}", venta.NumeroVenta);
            return venta.ID_Venta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CrearManualAsync");
            return null;
        }
    }

    public async Task<bool> MarkupSaleComplete(int idVenta)
    {
        try
        {
            var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta);
            if (venta == null) return false;

            venta.EstadoVenta = "COMPLETADA";
            venta.FechaModificacion = DateTime.Now;

            await _ventaRepository.ActualizarAsync(venta);
            _logger.LogInformation("Venta marcada como completada: {Id}", idVenta);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en MarkupSaleComplete");
            return false;
        }
    }

    public async Task<decimal> ObtenerTotalVendidoAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            var ventas = await ObtenerPorFechaAsync(desde, hasta);
            return ventas.Sum(v => v.TotalVenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerTotalVendidoAsync");
            return 0;
        }
    }

    private static VentaDTO MapearDTO(Venta venta)
    {
        return new VentaDTO
        {
            IdVenta = venta.ID_Venta,
            NumeroVenta = venta.NumeroVenta,
            IdCliente = venta.ID_Cliente,
            ClienteNombre = venta.Cliente?.Nombre ?? "Desconocido",
            IdVehiculo = venta.ID_Vehiculo,
            VehiculoPlaca = venta.Vehiculo?.Placa,
            PesoTaraKg = venta.PesoTaraKg ?? 0,
            PesoBrutoKg = venta.PesoBrutoKg ?? 0,
            PesoNetoKg = venta.PesoNetoKg ?? 0,
            Subtotal = venta.Subtotal,
            DescuentosAplicados = venta.DescuentosAplicados,
            IVA = venta.IVA,
            TotalVenta = venta.TotalVenta,
            TipoDocumento = venta.TipoDocumento,
            Estado = venta.EstadoVenta,
            FechaVenta = venta.FechaVenta
        };
    }
}
