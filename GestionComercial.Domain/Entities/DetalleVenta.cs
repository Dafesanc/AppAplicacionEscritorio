namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad DetalleVenta - Líneas de detalle de cada venta
/// </summary>
public class DetalleVenta
{
    public int ID_DetalleVenta { get; set; }
    public int ID_Venta { get; set; }
    public int ID_Producto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoLinea { get; set; } = 0; // Porcentaje
    public decimal? ValorDescuento { get; set; }
    public decimal? SubtotalLinea { get; set; }

    // Relaciones
    public Venta Venta { get; set; } = null!;
    public Producto Producto { get; set; } = null!;

    /// <summary>
    /// Calcula el subtotal de la línea
    /// </summary>
    public void CalcularSubtotal()
    {
        var subtotal = Cantidad * PrecioUnitario;
        ValorDescuento = subtotal * (DescuentoLinea / 100);
        SubtotalLinea = subtotal - ValorDescuento;
    }
}
