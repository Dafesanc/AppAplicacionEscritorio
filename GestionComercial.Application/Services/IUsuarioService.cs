namespace GestionComercial.Application.Services;

using GestionComercial.Application.DTOs;

/// <summary>
/// Interfaz para el servicio de Usuario
/// </summary>
public interface IUsuarioService
{
    Task<LoginResponseDTO?> AutenticarAsync(LoginDTO loginDTO);
    Task<IEnumerable<UsuarioDTO>> ObtenerActivosAsync();
    Task<UsuarioDTO?> ObtenerPorIdAsync(int idUsuario);
    Task<UsuarioDTO?> ObtenerPorNombreAsync(string nombreUsuario);
    Task<int?> CrearAsync(CrearActualizarUsuarioDTO usuarioDTO);
    Task<bool> ActualizarAsync(int idUsuario, CrearActualizarUsuarioDTO usuarioDTO);
    Task<bool> InactivarAsync(int idUsuario);
}
