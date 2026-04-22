namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Factura - Comprobantes formales de venta
/// </summary>
public class Factura
{
    public int ID_Factura { get; set; }
    public string NumeroFactura { get; set; } = null!;
    public int ID_Venta { get; set; }
    public int ID_Cliente { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal? SubtotalFactura { get; set; }
    public decimal? IVAFactura { get; set; }
    public decimal? TotalFactura { get; set; }
    public string EstadoFactura { get; set; } = "EMITIDA"; // EMITIDA, PAGADA, ANULADA, CRÉDITO
    public string? ObservacionesFactura { get; set; }
    public int UsuarioEmision { get; set; }

    // Relaciones
    public Venta Venta { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;

    /// <summary>
    /// Verifica si la factura está emitida
    /// </summary>
    public bool EstaEmitida => EstadoFactura == "EMITIDA";

    /// <summary>
    /// Verifica si la factura está pagada
    /// </summary>
    public bool EstaPagada => EstadoFactura == "PAGADA";

    /// <summary>
    /// Verifica si la factura está anulada
    /// </summary>
    public bool EstaAnulada => EstadoFactura == "ANULADA";

    /// <summary>
    /// Obtiene el estado formateado
    /// </summary>
    public string ObtenerEstadoFormateado => EstadoFactura switch
    {
        "EMITIDA" => "Emitida",
        "PAGADA" => "Pagada",
        "ANULADA" => "Anulada",
        "CRÉDITO" => "Crédito",
        _ => EstadoFactura
    };
}
