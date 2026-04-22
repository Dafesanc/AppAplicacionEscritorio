namespace GestionComercial.Application.DTOs;

/// <summary>
/// DTO para Cliente
/// </summary>
public class ClienteDTO
{
    public int IdCliente { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public decimal DescuentoPorDefecto { get; set; }
    public int PlazoCredito { get; set; }
    public decimal LimiteCredito { get; set; }
    public decimal SaldoCredito { get; set; }
    public decimal CreditoDisponible => LimiteCredito - SaldoCredito;
    public string Estado { get; set; } = "ACTIVO";
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para crear/actualizar Cliente
/// </summary>
public class CrearActualizarClienteDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public decimal DescuentoPorDefecto { get; set; }
    public int PlazoCredito { get; set; }
    public decimal LimiteCredito { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para Venta
/// </summary>
public class VentaDTO
{
    public int IdVenta { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int? IdVehiculo { get; set; }
    public string? VehiculoPlaca { get; set; }
    public decimal PesoTaraKg { get; set; }
    public decimal PesoBrutoKg { get; set; }
    public decimal PesoNetoKg { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentosAplicados { get; set; }
    public decimal IVA { get; set; }
    public decimal TotalVenta { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }
}

/// <summary>
/// DTO para crear Venta
/// </summary>
public class CrearVentaDTO
{
    public int IdCliente { get; set; }
    public int IdVehiculo { get; set; }
    public int IdPesajeTara { get; set; }
    public int IdPesajeBruto { get; set; }
    public List<DetalleVentaDTO> Detalles { get; set; } = new();
    public string TipoDocumento { get; set; } = "TICKET";
}

/// <summary>
/// DTO para detalle de venta
/// </summary>
public class DetalleVentaDTO
{
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal? DescuentoLinea { get; set; }
}

/// <summary>
/// DTO para Producto
/// </summary>
public class ProductoDTO
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoMaterial { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal Stock { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    public int Deficit => Stock < StockMinimo ? (int)(StockMinimo - Stock) : 0;
    public decimal ValorInventario => Stock * PrecioBase;
}

/// <summary>
/// DTO para Movimiento Inventario
/// </summary>
public class MovimientoInventarioDTO
{
    public int IdMovimiento { get; set; }
    public int IdProducto { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal StockAnterior { get; set; }
    public decimal StockPosterior { get; set; }
    public string? Referencia { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string? UsuarioNombre { get; set; }
}

/// <summary>
/// DTO para Pesaje
/// </summary>
public class PesajeDTO
{
    public int IdPesaje { get; set; }
    public int IdVehiculo { get; set; }
    public string VehiculoPlaca { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;  // TARA, BRUTO
    public decimal PesoKg { get; set; }
    public decimal PesoToneladas => PesoKg / 1000;
    public DateTime FechaPesaje { get; set; }
    public string? ObservacionesBascula { get; set; }
}

/// <summary>
/// DTO para registrar Pesaje desde báscula
/// </summary>
public class RegistrarPesajeDTO
{
    public int IdVehiculo { get; set; }
    public string Tipo { get; set; } = string.Empty;  // TARA, BRUTO
    public decimal PesoKg { get; set; }
    public decimal? Temperatura { get; set; }
    public decimal? Humedad { get; set; }
    public string? EstadoBascula { get; set; }
}

/// <summary>
/// DTO para Vehículo
/// </summary>
public class VehiculoDTO
{
    public int IdVehiculo { get; set; }
    public int IdCliente { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Color { get; set; }
    public decimal? CapacidadTon { get; set; }
    public int? AnoFabricacion { get; set; }
    public decimal? PesoTaraKg { get; set; }
    public string? VIN { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    public DateTime? UltimaPesada { get; set; }
    public string? Observaciones { get; set; }
    public bool EstaActivo => Estado == "ACTIVO";
    public string DescripcionCompleta => $"{Marca} {Modelo} ({Placa})".Trim();
}

/// <summary>
/// DTO para crear/actualizar Vehículo
/// </summary>
public class CrearActualizarVehiculoDTO
{
    public int IdCliente { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Color { get; set; }
    public decimal? CapacidadTon { get; set; }
    public int? AnoFabricacion { get; set; }
    public decimal? PesoTaraKg { get; set; }
    public string? VIN { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para Factura
/// </summary>
public class FacturaDTO
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public int IdVenta { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal SubtotalFactura { get; set; }
    public decimal IVAFactura { get; set; }
    public decimal TotalFactura { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal MontoPendiente => TotalFactura - MontoPagado;
    public string Estado { get; set; } = string.Empty;
    public int? DiasVencimiento => FechaVencimiento.HasValue 
        ? (FechaVencimiento.Value.Date - DateTime.Now.Date).Days 
        : null;
}
