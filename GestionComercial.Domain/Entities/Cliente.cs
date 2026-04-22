namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Cliente - Clientes de la empresa (transportistas, mayoristas, etc.)
/// </summary>
public class Cliente
{
    public int ID_Cliente { get; set; }
    public string CodigoCliente { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string TipoIdentificacion { get; set; } = null!; // RUC, Cédula, Pasaporte
    public string NumeroIdentificacion { get; set; } = null!;
    public string Categoria { get; set; } = null!; // Transportista, Mayorista, Minorista
    public string? Contacto { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public decimal DescuentoPorDefecto { get; set; } = 0;
    public int PlazoCredito { get; set; } = 0;
    public decimal LimiteCredito { get; set; } = 0;
    public decimal SaldoCredito { get; set; } = 0;
    public string Estado { get; set; } = "ACTIVO"; // ACTIVO, INACTIVO, BLOQUEADO
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime FechaModificacion { get; set; }
    public int? UsuarioCreacion { get; set; }

    // Relaciones
    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public Usuario? UsuarioCreador { get; set; }

    /// <summary>
    /// Verifica si el cliente está activo
    /// </summary>
    public bool EstaActivo => Estado == "ACTIVO";

    /// <summary>
    /// Verifica si el cliente está bloqueado
    /// </summary>
    public bool EstaBloqueado => Estado == "BLOQUEADO";

    /// <summary>
    /// Calcula el crédito disponible
    /// </summary>
    public decimal CreditoDisponible => LimiteCredito - SaldoCredito;

    /// <summary>
    /// Verifica si el cliente tiene crédito disponible
    /// </summary>
    public bool TieneCreditoDisponible => CreditoDisponible > 0;
}
