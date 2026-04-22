namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Rol - Define los roles del sistema
/// </summary>
public class Rol
{
    public int ID_Rol { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }

    // Relaciones
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
