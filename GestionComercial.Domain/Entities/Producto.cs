namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Producto - Catálogo de productos comercializados
/// </summary>
public class Producto
{
    public int ID_Producto { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string TipoMaterial { get; set; } = null!; // Arena, Piedra, Cemento, etc.
    public string Unidad { get; set; } = null!; // Kg, Tonelada, Unidad, Metro, Metro2, Metro3
    public decimal PrecioBase { get; set; }
    public decimal Stock { get; set; } = 0;
    public decimal StockMinimo { get; set; } = 0;
    public decimal StockMaximo { get; set; } = 0;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "ACTIVO"; // ACTIVO, INACTIVO
    public DateTime FechaRegistro { get; set; }
    public DateTime FechaModificacion { get; set; }
    public int? UsuarioCreacion { get; set; }

    // Relaciones
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    public ICollection<LoteInventario> Lotes { get; set; } = new List<LoteInventario>();
    public Usuario? UsuarioCreador { get; set; }

    /// <summary>
    /// Verifica si el producto está activo
    /// </summary>
    public bool EstaActivo => Estado == "ACTIVO";

    /// <summary>
    /// Verifica si el stock está bajo del mínimo
    /// </summary>
    public bool StockEsBajo => Stock < StockMinimo;

    /// <summary>
    /// Calcula cuántas unidades faltan para alcanzar el mínimo
    /// </summary>
    public decimal DeficitStock => StockMinimo - Stock;

    /// <summary>
    /// Verifica si hay espacio para más stock
    /// </summary>
    public bool HayEspacioStock => Stock < StockMaximo;

    /// <summary>
    /// Calcula el promedio ponderado del stock
    /// </summary>
    public decimal ValorInventario => Stock * PrecioBase;
}
