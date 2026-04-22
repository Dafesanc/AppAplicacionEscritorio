namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad MovimientoInventario - Auditoría de movimientos de stock
/// </summary>
public class MovimientoInventario
{
    public int ID_Movimiento { get; set; }
    public int ID_Producto { get; set; }
    public string TipoMovimiento { get; set; } = null!; // ENTRADA, SALIDA, AJUSTE
    public decimal Cantidad { get; set; }
    public decimal? StockAnterior { get; set; }
    public decimal? StockPosterior { get; set; }
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public int UsuarioMovimiento { get; set; }

    // Relaciones
    public Producto Producto { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;

    /// <summary>
    /// Verifica si es entrada de stock
    /// </summary>
    public bool EsEntrada => TipoMovimiento == "ENTRADA";

    /// <summary>
    /// Verifica si es salida de stock
    /// </summary>
    public bool EsSalida => TipoMovimiento == "SALIDA";

    /// <summary>
    /// Verifica si es ajuste de stock
    /// </summary>
    public bool EsAjuste => TipoMovimiento == "AJUSTE";

    /// <summary>
    /// Obtiene el impacto del movimiento
    /// </summary>
    public decimal ObtenerImpacto => TipoMovimiento switch
    {
        "ENTRADA" => Cantidad,
        "SALIDA" => -Cantidad,
        "AJUSTE" => Cantidad, // puede ser positivo o negativo
        _ => 0
    };
}
