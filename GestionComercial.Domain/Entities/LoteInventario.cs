namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad LoteInventario - Control de lotes de productos (FIFO)
/// </summary>
public class LoteInventario
{
    public int ID_Lote { get; set; }
    public int ID_Producto { get; set; }
    public string NumeroLote { get; set; } = null!;
    public DateTime? FechaFabricacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal QuantidadRecibida { get; set; }
    public decimal QuantidadDisponible { get; set; }
    public string Estado { get; set; } = "DISPONIBLE"; // DISPONIBLE, AGOTADO, EXPIRADO
    public string? Proveedor { get; set; }
    public DateTime FechaIngreso { get; set; }

    // Relaciones
    public Producto Producto { get; set; } = null!;

    /// <summary>
    /// Verifica si el lote está disponible
    /// </summary>
    public bool EstaDisponible => Estado == "DISPONIBLE";

    /// <summary>
    /// Verifica si el lote está agotado
    /// </summary>
    public bool EstaAgotado => Estado == "AGOTADO";

    /// <summary>
    /// Verifica si el lote está vencido
    /// </summary>
    public bool EstaVencido => FechaVencimiento.HasValue && FechaVencimiento.Value < DateTime.Now;

    /// <summary>
    /// Calcula la cantidad utilizada del lote
    /// </summary>
    public decimal CantidadUtilizada => QuantidadRecibida - QuantidadDisponible;

    /// <summary>
    /// Obtiene el porcentaje de utilización
    /// </summary>
    public decimal PorcentajeUtilizacion => QuantidadRecibida > 0 ? (CantidadUtilizada / QuantidadRecibida) * 100 : 0;
}
