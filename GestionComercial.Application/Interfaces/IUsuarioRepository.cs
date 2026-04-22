namespace GestionComercial.Application.Interfaces;

using GestionComercial.Domain.Entities;

/// <summary>
/// Interfaz para el repositorio de Usuarios
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario);
    Task<IEnumerable<Usuario>> ObtenerPorRolAsync(int idRol);
    Task<IEnumerable<Usuario>> ObtenerActivosAsync();
    Task<bool> ExisteNombreAsync(string nombreUsuario);
}
