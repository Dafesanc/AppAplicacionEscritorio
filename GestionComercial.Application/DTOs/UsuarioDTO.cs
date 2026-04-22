namespace GestionComercial.Application.DTOs;

/// <summary>
/// DTO para Usuario - para consultas
/// </summary>
public class UsuarioDTO
{
    public int ID_Usuario { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public int ID_Rol { get; set; }
    public string RolNombre { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime? UltimoLogin { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// DTO para crear/actualizar Usuario
/// </summary>
public class CrearActualizarUsuarioDTO
{
    public string NombreUsuario { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Telefono { get; set; }
    public int ID_Rol { get; set; }
    public string? Contrasena { get; set; } // Solo para creación, no enviar hash
}

/// <summary>
/// DTO para Login
/// </summary>
public class LoginDTO
{
    public string NombreUsuario { get; set; } = null!;
    public string Contrasena { get; set; } = null!;
}

/// <summary>
/// DTO para respuesta de Login
/// </summary>
public class LoginResponseDTO
{
    public int ID_Usuario { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public int ID_Rol { get; set; }
    public string RolNombre { get; set; } = null!;
    public string Token { get; set; } = null!; // JWT (futuro)
}
