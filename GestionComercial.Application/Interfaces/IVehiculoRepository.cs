namespace GestionComercial.Application.Interfaces;

using GestionComercial.Domain.Entities;

/// <summary>
/// Interfaz para el repositorio de Vehículo
/// </summary>
public interface IVehiculoRepository : IRepository<Vehiculo>
{
    Task<Vehiculo?> ObtenerPorPlacaAsync(string placa);
    Task<IEnumerable<Vehiculo>> ObtenerPorClienteAsync(int idCliente);
    Task<IEnumerable<Vehiculo>> ObtenerActivosPorClienteAsync(int idCliente);
    Task<IEnumerable<Vehiculo>> ObtenerActivosAsync();
    Task<bool> ExistePlacaAsync(string placa);
    Task<IEnumerable<Vehiculo>> ObtenerEnMantenimientoAsync();
    Task<Pesaje?> ObtenerUltimoPesajeAsync(int idVehiculo);
}
