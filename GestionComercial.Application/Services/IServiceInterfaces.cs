namespace GestionComercial.Application.Services;

using GestionComercial.Application.DTOs;

/// <summary>
/// Interfaz para el servicio de Cliente
/// </summary>
public interface IClienteService
{
    Task<ClienteDTO?> ObtenerPorIdAsync(int idCliente);
    Task<ClienteDTO?> ObtenerPorIdentificacionAsync(string numeroIdentificacion);
    Task<IEnumerable<ClienteDTO>> ObtenerActivosAsync();
    Task<IEnumerable<ClienteDTO>> ObtenerClientesFrecuentesAsync(int top = 10);
    Task<decimal> ObtenerCreditoDisponibleAsync(int idCliente);
    Task<int?> CrearAsync(CrearActualizarClienteDTO clienteDTO);
    Task<bool> ActualizarAsync(int idCliente, CrearActualizarClienteDTO clienteDTO);
    Task<bool> BloquearAsync(int idCliente);
    Task<bool> DesbloquearAsync(int idCliente);
}

/// <summary>
/// Interfaz para el servicio de Venta
/// </summary>
public interface IVentaService
{
    Task<VentaDTO?> ObtenerPorIdAsync(int idVenta);
    Task<VentaDTO?> ObtenerPorNumeroAsync(string numeroVenta);
    Task<IEnumerable<VentaDTO>> ObtenerPorClienteAsync(int idCliente);
    Task<IEnumerable<VentaDTO>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta);
    Task<int?> CrearAsync(CrearVentaDTO ventaDTO);
    Task<bool> MarkupSaleComplete(int idVenta);
    Task<decimal> ObtenerTotalVendidoAsync(DateTime desde, DateTime hasta);
}

/// <summary>
/// Interfaz para el servicio de Inventario
/// </summary>
public interface IInventarioService
{
    Task<ProductoDTO?> ObtenerPorIdAsync(int idProducto);
    Task<IEnumerable<ProductoDTO>> ObtenerStockBajoAsync();
    Task<IEnumerable<ProductoDTO>> ObtenerActivosAsync();
    Task<bool> ActualizarStockAsync(int idProducto, decimal cantidad, string tipo, string referencia);
    Task<bool> AjustarStockAsync(int idProducto, decimal cantidadAjuste, string razon);
    Task<decimal> ObtenerValorInventarioTotalAsync();
    Task<IEnumerable<MovimientoInventarioDTO>> ObtenerMovimientosAsync(int idProducto, int? diasAtras = null);
}

/// <summary>
/// Interfaz para el servicio de Pesaje/Báscula
/// </summary>
public interface IPesajeService
{
    Task<PesajeDTO?> ObtenerPorIdAsync(int idPesaje);
    Task<PesajeDTO?> ObtenerUltimoPesajeAsync(int idVehiculo);
    Task<IEnumerable<PesajeDTO>> ObtenerHistorialVehiculoAsync(int idVehiculo, int limitUltimos = 10);
    Task<IEnumerable<PesajeDTO>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta);
    Task<int?> RegistrarPesajeAsync(RegistrarPesajeDTO pesajeDTO);
    Task<(decimal PesoNeto, PesajeDTO? Tara, PesajeDTO? Bruto)?> ObtenerPesoNetoAsync(int idTara, int idBruto);
    Task ConectarBasculaAsync(string puerto, int baudRate = 9600);
    Task DesconectarBasculaAsync();
}

/// <summary>
/// Interfaz para el servicio de Vehículo
/// </summary>
public interface IVehiculoService
{
    Task<VehiculoDTO?> ObtenerPorIdAsync(int idVehiculo);
    Task<VehiculoDTO?> ObtenerPorPlacaAsync(string placa);
    Task<IEnumerable<VehiculoDTO>> ObtenerPorClienteAsync(int idCliente);
    Task<IEnumerable<VehiculoDTO>> ObtenerActivosAsync();
    Task<int?> CrearAsync(CrearActualizarVehiculoDTO dto);
    Task<bool> ActualizarAsync(int idVehiculo, CrearActualizarVehiculoDTO dto);
    Task<bool> CambiarEstadoAsync(int idVehiculo, string nuevoEstado);
}

/// <summary>
/// Interfaz para el servicio de Factura
/// </summary>
public interface IFacturaService
{
    Task<FacturaDTO?> ObtenerPorIdAsync(int idFactura);
    Task<FacturaDTO?> ObtenerPorNumeroAsync(string numeroFactura);
    Task<IEnumerable<FacturaDTO>> ObtenerPendientesPagoAsync(int? idCliente = null);
    Task<IEnumerable<FacturaDTO>> ObtenerVencidasAsync(int diasVencimiento = 30);
    Task<int?> GenerarFacturaAsync(int idVenta);
    Task<bool> MarcarPagadaAsync(int idFactura, decimal montoRecibido);
    Task<decimal> ObtenerTotalPendientePagoAsync(int? idCliente = null);
}
