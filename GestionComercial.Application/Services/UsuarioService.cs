namespace GestionComercial.Application.Services;

using BCrypt.Net;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.Extensions.Logging;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(IUsuarioRepository usuarioRepository, ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    public async Task<LoginResponseDTO?> AutenticarAsync(LoginDTO loginDTO)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginDTO.NombreUsuario) ||
                string.IsNullOrWhiteSpace(loginDTO.Contrasena))
            {
                _logger.LogWarning("Intento de login con credenciales incompletas");
                return null;
            }

            var usuario = await _usuarioRepository.ObtenerPorNombreAsync(loginDTO.NombreUsuario);

            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado: {Usuario}", loginDTO.NombreUsuario);
                return null;
            }

            if (usuario.EstaBloqueado)
            {
                _logger.LogWarning("Usuario bloqueado: {Usuario}", loginDTO.NombreUsuario);
                return null;
            }

            if (usuario.Estado != "ACTIVO")
            {
                _logger.LogWarning("Usuario inactivo: {Usuario}", loginDTO.NombreUsuario);
                return null;
            }

            // Verificación con BCrypt. Compatibilidad retroactiva: si el hash almacenado
            // no es BCrypt (no empieza por "$2"), se compara como texto plano y se migra.
            bool contrasenaValida;
            if (EsHashBcrypt(usuario.Contrasena))
            {
                contrasenaValida = BCrypt.Verify(loginDTO.Contrasena, usuario.Contrasena);
            }
            else
            {
                contrasenaValida = usuario.Contrasena == loginDTO.Contrasena;
                if (contrasenaValida)
                {
                    // Migrar a BCrypt en el primer login exitoso
                    usuario.Contrasena = BCrypt.HashPassword(loginDTO.Contrasena, workFactor: 12);
                    usuario.FechaModificacion = DateTime.Now;
                    await _usuarioRepository.ActualizarAsync(usuario);
                    _logger.LogInformation("Contraseña migrada a BCrypt: {Usuario}", loginDTO.NombreUsuario);
                }
            }

            if (!contrasenaValida)
            {
                usuario.IntentosFallidos++;
                if (usuario.DebeSerBloqueado)
                    usuario.Estado = "BLOQUEADO";
                usuario.FechaModificacion = DateTime.Now;
                await _usuarioRepository.ActualizarAsync(usuario);
                _logger.LogWarning("Contraseña incorrecta para: {Usuario}", loginDTO.NombreUsuario);
                return null;
            }

            // Resetear intentos fallidos en login exitoso
            if (usuario.IntentosFallidos > 0)
            {
                usuario.IntentosFallidos = 0;
                usuario.UltimoLogin = DateTime.Now;
                usuario.FechaModificacion = DateTime.Now;
                await _usuarioRepository.ActualizarAsync(usuario);
            }

            _logger.LogInformation("Login exitoso: {Usuario}", loginDTO.NombreUsuario);

            return new LoginResponseDTO
            {
                ID_Usuario = usuario.ID_Usuario,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                ID_Rol = usuario.ID_Rol,
                RolNombre = usuario.Rol?.Nombre ?? "Sin rol",
                Token = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en AutenticarAsync");
            return null;
        }
    }

    public async Task<IEnumerable<UsuarioDTO>> ObtenerActivosAsync()
    {
        try
        {
            var usuarios = await _usuarioRepository.ObtenerActivosAsync();
            return usuarios.Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerActivosAsync");
            return Enumerable.Empty<UsuarioDTO>();
        }
    }

    public async Task<UsuarioDTO?> ObtenerPorIdAsync(int idUsuario)
    {
        try
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(idUsuario);
            return usuario == null ? null : MapearDTO(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorIdAsync");
            return null;
        }
    }

    public async Task<UsuarioDTO?> ObtenerPorNombreAsync(string nombreUsuario)
    {
        try
        {
            var usuario = await _usuarioRepository.ObtenerPorNombreAsync(nombreUsuario);
            return usuario == null ? null : MapearDTO(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorNombreAsync");
            return null;
        }
    }

    public async Task<int?> CrearAsync(CrearActualizarUsuarioDTO usuarioDTO)
    {
        try
        {
            if (await _usuarioRepository.ExisteNombreAsync(usuarioDTO.NombreUsuario))
            {
                _logger.LogWarning("Intento de crear usuario duplicado: {Usuario}", usuarioDTO.NombreUsuario);
                return null;
            }

            var hashContrasena = !string.IsNullOrWhiteSpace(usuarioDTO.Contrasena)
                ? BCrypt.HashPassword(usuarioDTO.Contrasena, workFactor: 12)
                : BCrypt.HashPassword("Cambiar123!", workFactor: 12); // contraseña temporal

            var usuario = new Usuario
            {
                NombreUsuario = usuarioDTO.NombreUsuario,
                NombreCompleto = usuarioDTO.NombreCompleto,
                Contrasena = hashContrasena,
                Email = usuarioDTO.Email,
                Telefono = usuarioDTO.Telefono,
                ID_Rol = usuarioDTO.ID_Rol,
                Estado = "ACTIVO",
                IntentosFallidos = 0,
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now
            };

            await _usuarioRepository.AgregarAsync(usuario);
            _logger.LogInformation("Usuario creado: {Usuario}", usuarioDTO.NombreUsuario);
            return usuario.ID_Usuario;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CrearAsync");
            return null;
        }
    }

    public async Task<bool> ActualizarAsync(int idUsuario, CrearActualizarUsuarioDTO usuarioDTO)
    {
        try
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(idUsuario);
            if (usuario == null) return false;

            usuario.NombreCompleto = usuarioDTO.NombreCompleto;
            usuario.Email = usuarioDTO.Email;
            usuario.Telefono = usuarioDTO.Telefono;
            usuario.ID_Rol = usuarioDTO.ID_Rol;
            usuario.FechaModificacion = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(usuarioDTO.Contrasena))
                usuario.Contrasena = BCrypt.HashPassword(usuarioDTO.Contrasena, workFactor: 12);

            await _usuarioRepository.ActualizarAsync(usuario);
            _logger.LogInformation("Usuario actualizado: {Id}", idUsuario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ActualizarAsync");
            return false;
        }
    }

    public async Task<bool> InactivarAsync(int idUsuario)
    {
        try
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(idUsuario);
            if (usuario == null) return false;

            usuario.Estado = "INACTIVO";
            usuario.FechaModificacion = DateTime.Now;

            await _usuarioRepository.ActualizarAsync(usuario);
            _logger.LogInformation("Usuario inactivado: {Id}", idUsuario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en InactivarAsync");
            return false;
        }
    }

    // Detecta si el valor almacenado ya es un hash BCrypt ($2a$, $2b$, $2x$, $2y$)
    private static bool EsHashBcrypt(string valor)
        => !string.IsNullOrEmpty(valor) && valor.StartsWith("$2", StringComparison.Ordinal) && valor.Length >= 60;

    private static UsuarioDTO MapearDTO(Usuario u) => new()
    {
        ID_Usuario = u.ID_Usuario,
        NombreUsuario = u.NombreUsuario,
        NombreCompleto = u.NombreCompleto,
        Email = u.Email ?? string.Empty,
        Telefono = u.Telefono ?? string.Empty,
        ID_Rol = u.ID_Rol,
        RolNombre = u.Rol?.Nombre ?? "Sin rol",
        Estado = u.Estado,
        UltimoLogin = u.UltimoLogin
    };
}
