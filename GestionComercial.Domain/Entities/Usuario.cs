namespace GestionComercial.Domain.Entities;

/// <summary>
/// Entidad Usuario - Usuarios del sistema con autenticación
/// </summary>
public class Usuario
{
    public int ID_Usuario { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Contrasena { get; set; } = null!; // Hash bcrypt
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public int ID_Rol { get; set; }
    public string Estado { get; set; } = "ACTIVO"; // ACTIVO, INACTIVO, BLOQUEADO
    public DateTime? UltimoLogin { get; set; }
    public int IntentosFallidos { get; set; } = 0;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }

    // Relaciones
    public Rol Rol { get; set; } = null!;

    /// <summary>
    /// Valida que el usuario esté activo
    /// </summary>
    public bool EstaActivo => Estado == "ACTIVO";

    /// <summary>
    /// Valida que el usuario no esté bloqueado
    /// </summary>
    public bool EstaBloqueado => Estado == "BLOQUEADO";

    /// <summary>
    /// Verifica si la cuenta está bloqueada por intentos fallidos
    /// </summary>
    public bool DebeSerBloqueado => IntentosFallidos >= 5;
}
