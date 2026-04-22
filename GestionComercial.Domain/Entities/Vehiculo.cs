namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Vehículo - Vehículos asociados a clientes (máximo 2 por cliente)
/// </summary>
public class Vehiculo
{
    public int ID_Vehiculo { get; set; }
    public int ID_Cliente { get; set; }
    public string Placa { get; set; } = null!;
    public string Tipo { get; set; } = null!; // Volqueta, Gandola, Furgón, etc.
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Color { get; set; }
    public decimal? Capacidad { get; set; } // Toneladas
    public int? AnoFabricacion { get; set; }
    public decimal? PesoTara { get; set; } // kg - Peso conocido del vehículo vacío
    public string? VIN { get; set; }
    public string? PlacaINEN { get; set; }
    public string Estado { get; set; } = "ACTIVO"; // ACTIVO, INACTIVO, MANTENIMIENTO
    public DateTime? UltimaPesada { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime FechaModificacion { get; set; }
    public int? UsuarioCreacion { get; set; }

    // Relaciones
    public Cliente Cliente { get; set; } = null!;
    public ICollection<Pesaje> Pesajes { get; set; } = new List<Pesaje>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public Usuario? UsuarioCreador { get; set; }

    /// <summary>
    /// Verifica si el vehículo está activo
    /// </summary>
    public bool EstaActivo => Estado == "ACTIVO";

    /// <summary>
    /// Obtiene la descripción del vehículo
    /// </summary>
    public string ObtenerDescripcion => $"{Marca} {Modelo} - Placa: {Placa}";
}
