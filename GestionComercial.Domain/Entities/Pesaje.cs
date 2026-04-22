namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Pesaje - Registro de pesajes desde la báscula (RS232)
/// </summary>
public class Pesaje
{
    public int ID_Pesaje { get; set; }
    public int ID_Vehiculo { get; set; }
    public string TipoPesaje { get; set; } = null!; // TARA, BRUTO
    public decimal PesoKg { get; set; }
    public decimal? Temperatura { get; set; }
    public decimal? Humedad { get; set; }
    public string? EstadoBascula { get; set; }
    public string? NumeroSerie { get; set; }
    public DateTime FechaPesaje { get; set; }
    public int UsuarioPesaje { get; set; }
    public string? Observaciones { get; set; }

    // Relaciones
    public Vehiculo Vehiculo { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;

    /// <summary>
    /// Determina si es pesaje de TARA
    /// </summary>
    public bool EsTara => TipoPesaje == "TARA";

    /// <summary>
    /// Determina si es pesaje de BRUTO
    /// </summary>
    public bool EsBruto => TipoPesaje == "BRUTO";

    /// <summary>
    /// Convierte peso a toneladas
    /// </summary>
    public decimal PesoToneladas => PesoKg / 1000;
}
