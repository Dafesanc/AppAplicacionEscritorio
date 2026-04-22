namespace GestionComercial.Infrastructure.Persistence.Repositories;

using GestionComercial.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implementación genérica del patrón Repository
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly GestionComercialContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(GestionComercialContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Obtiene una entidad por su ID
    /// </summary>
    public async Task<T?> ObtenerPorIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Obtiene todas las entidades
    /// </summary>
    public async Task<IEnumerable<T>> ObtenerTodosAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Obtiene entidades con filtro (carga todos los registros y filtra en memoria)
    /// </summary>
    public async Task<IEnumerable<T>> ObtenerConFiltroAsync(Func<T, bool> predicado)
    {
        var todos = await _dbSet.ToListAsync();
        return todos.Where(predicado);
    }

    /// <summary>
    /// Agrega una nueva entidad
    /// </summary>
    public async Task AgregarAsync(T entidad)
    {
        await _dbSet.AddAsync(entidad);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza una entidad existente
    /// </summary>
    public async Task ActualizarAsync(T entidad)
    {
        _dbSet.Update(entidad);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina una entidad por ID
    /// </summary>
    public async Task EliminarAsync(int id)
    {
        var entidad = await ObtenerPorIdAsync(id);
        if (entidad != null)
        {
            _dbSet.Remove(entidad);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Verifica si existe una entidad
    /// </summary>
    public async Task<bool> ExisteAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}
