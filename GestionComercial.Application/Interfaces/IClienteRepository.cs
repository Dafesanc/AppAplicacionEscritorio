namespace GestionComercial.Application.Interfaces;

using GestionComercial.Domain.Entities;

/// <summary>
/// Interfaz para el repositorio de Cliente
/// </summary>
public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> ObtenerPorIdentificacionAsync(string numeroIdentificacion);
    Task<Cliente?> ObtenerPorCodigoAsync(string codigo);
    Task<IEnumerable<Cliente>> ObtenerActivosAsync();
    Task<IEnumerable<Cliente>> ObtenerPorCategoriaAsync(string categoria);
    Task<IEnumerable<Cliente>> ObtenerClientesFrecuentesAsync(int top = 10, int? limitMeses = null);
    Task<IEnumerable<Cliente>> ObtenerConCreditoDisponibleAsync();
    Task<bool> ExisteIdentificacionAsync(string numeroIdentificacion);
    Task<Cliente?> ObtenerConHistorialAsync(int idCliente);
}
