namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Venta - Transacciones de venta realizadas
/// </summary>
public class Venta
{
    public int ID_Venta { get; set; }
    public string NumeroVenta { get; set; } = null!;
    public int ID_Cliente { get; set; }
    public int? ID_Vehiculo { get; set; }
    public int? ID_PesajeTara { get; set; }
    public int? ID_PesajeBruto { get; set; }
    public decimal? PesoTaraKg { get; set; }
    public decimal? PesoBrutoKg { get; set; }
    public decimal? PesoNetoKg { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentosAplicados { get; set; } = 0;
    public decimal IVA { get; set; } = 0;
    public decimal TotalVenta { get; set; }
    public string TipoDocumento { get; set; } = "TICKET"; // TICKET, FACTURA
    public string EstadoVenta { get; set; } = "COMPLETADA"; // BORRADOR, COMPLETADA, ANULADA
    public int UsuarioVenta { get; set; }
    public DateTime FechaVenta { get; set; }
    public DateTime FechaModificacion { get; set; }
    public int? ID_Producto { get; set; } // Para ventas manuales sin pesaje

    // Relaciones
    public Cliente Cliente { get; set; } = null!;
    public Vehiculo? Vehiculo { get; set; }
    public Pesaje? PesajeTara { get; set; }
    public Pesaje? PesajeBruto { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    public Factura? Factura { get; set; }
    //Ultima Relacion
    public Producto? Producto { get; set; }

    /// <summary>
    /// Verifica si la venta está completada
    /// </summary>
    public bool EstaCompletada => EstadoVenta == "COMPLETADA";

    /// <summary>
    /// Verifica si la venta está anulada
    /// </summary>
    public bool EstaAnulada => EstadoVenta == "ANULADA";

    /// <summary>
    /// Verifica si es factura
    /// </summary>
    public bool EsFactura => TipoDocumento == "FACTURA";

    /// <summary>
    /// Verifica si es ticket
    /// </summary>
    public bool EsTicket => TipoDocumento == "TICKET";

    /// <summary>
    /// Calcula el peso neto automático
    /// </summary>
    public void CalcularPesoNeto()
    {
        if (PesoBrutoKg.HasValue && PesoTaraKg.HasValue)
        {
            PesoNetoKg = PesoBrutoKg.Value - PesoTaraKg.Value;
        }
    }

    /// <summary>
    /// Obtiene la cantidad de líneas de la venta
    /// </summary>
    public int ObtenerCantidadLineas => Detalles.Count;
}
