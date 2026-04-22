namespace GestionComercial.Application.Interfaces;

using GestionComercial.Domain.Entities;

/// <summary>
/// Interfaz genérica para el patrón Repository
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<T>> ObtenerTodosAsync();
    Task<IEnumerable<T>> ObtenerConFiltroAsync(Func<T, bool> predicado);
    Task AgregarAsync(T entidad);
    Task ActualizarAsync(T entidad);
    Task EliminarAsync(int id);
    Task <bool> ExisteAsync(int id);
}
