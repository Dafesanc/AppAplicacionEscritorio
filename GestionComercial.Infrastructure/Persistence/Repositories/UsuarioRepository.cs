namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio específico para Usuario con métodos personalizados
/// </summary>
public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(GestionComercialContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene un usuario por nombre
    /// </summary>
    public async Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario)
    {
        return await _dbSet
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    /// <summary>
    /// Obtiene usuarios por rol
    /// </summary>
    public async Task<IEnumerable<Usuario>> ObtenerPorRolAsync(int idRol)
    {
        return await _dbSet
            .Where(u => u.ID_Rol == idRol)
            .Include(u => u.Rol)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene todos los usuarios activos
    /// </summary>
    public async Task<IEnumerable<Usuario>> ObtenerActivosAsync()
    {
        return await _dbSet
            .Where(u => u.Estado == "ACTIVO")
            .Include(u => u.Rol)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe un nombre de usuario
    /// </summary>
    public async Task<bool> ExisteNombreAsync(string nombreUsuario)
    {
        return await _dbSet.AnyAsync(u => u.NombreUsuario == nombreUsuario);
    }
}
