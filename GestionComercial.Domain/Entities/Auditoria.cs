namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Auditoria - Registro de todas las operaciones del sistema
/// </summary>
public class Auditoria
{
    public int ID_Auditoria { get; set; }
    public int? ID_Usuario { get; set; }
    public string Tabla { get; set; } = null!;
    public string TipoOperacion { get; set; } = null!; // INSERT, UPDATE, DELETE
    public string? RegistroID { get; set; }
    public string? DatosAnteriores { get; set; } // JSON
    public string? DatosNuevos { get; set; } // JSON
    public DateTime FechaOperacion { get; set; }
    public string? DireccionIP { get; set; }
    public string? Razon { get; set; }

    // Relaciones
    public Usuario? Usuario { get; set; }

    /// <summary>
    /// Verifica si fue un INSERT
    /// </summary>
    public bool EsInsert => TipoOperacion == "INSERT";

    /// <summary>
    /// Verifica si fue un UPDATE
    /// </summary>
    public bool EsUpdate => TipoOperacion == "UPDATE";

    /// <summary>
    /// Verifica si fue un DELETE
    /// </summary>
    public bool EsDelete => TipoOperacion == "DELETE";

    /// <summary>
    /// Formatea la operación
    /// </summary>
    public string ObtenerOperacionFormateada => TipoOperacion switch
    {
        "INSERT" => "Creado",
        "UPDATE" => "Modificado",
        "DELETE" => "Eliminado",
        _ => TipoOperacion
    };
}
