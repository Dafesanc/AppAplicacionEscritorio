namespace GestionComercial.Application.Services;

using GestionComercial.Application.DTOs;
using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.Extensions.Logging;

public class VehiculoService : IVehiculoService
{
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IClienteRepository  _clienteRepository;
    private readonly ILogger<VehiculoService> _logger;

    public VehiculoService(
        IVehiculoRepository vehiculoRepository,
        IClienteRepository clienteRepository,
        ILogger<VehiculoService> logger)
    {
        _vehiculoRepository = vehiculoRepository;
        _clienteRepository  = clienteRepository;
        _logger             = logger;
    }

    public async Task<VehiculoDTO?> ObtenerPorIdAsync(int idVehiculo)
    {
        try
        {
            var v = await _vehiculoRepository.ObtenerPorIdAsync(idVehiculo);
            if (v == null) return null;
            var clienteNombre = await ObtenerNombreClienteAsync(v.ID_Cliente);
            return MapearDTO(v, clienteNombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorIdAsync vehiculo {Id}", idVehiculo);
            return null;
        }
    }

    public async Task<VehiculoDTO?> ObtenerPorPlacaAsync(string placa)
    {
        try
        {
            var v = await _vehiculoRepository.ObtenerPorPlacaAsync(placa);
            if (v == null) return null;
            var clienteNombre = await ObtenerNombreClienteAsync(v.ID_Cliente);
            return MapearDTO(v, clienteNombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorPlacaAsync {Placa}", placa);
            return null;
        }
    }

    public async Task<IEnumerable<VehiculoDTO>> ObtenerPorClienteAsync(int idCliente)
    {
        try
        {
            var vehiculos = await _vehiculoRepository.ObtenerPorClienteAsync(idCliente);
            var cliente   = await _clienteRepository.ObtenerPorIdAsync(idCliente);
            var nombre    = cliente?.Nombre ?? "N/A";
            return vehiculos.Select(v => MapearDTO(v, nombre));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorClienteAsync {IdCliente}", idCliente);
            return Enumerable.Empty<VehiculoDTO>();
        }
    }

    public async Task<IEnumerable<VehiculoDTO>> ObtenerActivosAsync()
    {
        try
        {
            var vehiculos = await _vehiculoRepository.ObtenerActivosAsync();
            var clientes  = new Dictionary<int, string>();

            foreach (var v in vehiculos)
            {
                if (!clientes.ContainsKey(v.ID_Cliente))
                {
                    var c = await _clienteRepository.ObtenerPorIdAsync(v.ID_Cliente);
                    clientes[v.ID_Cliente] = c?.Nombre ?? "N/A";
                }
            }

            return vehiculos.Select(v => MapearDTO(v, clientes[v.ID_Cliente]));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerActivosAsync");
            return Enumerable.Empty<VehiculoDTO>();
        }
    }

    public async Task<int?> CrearAsync(CrearActualizarVehiculoDTO dto)
    {
        try
        {
            if (await _vehiculoRepository.ExistePlacaAsync(dto.Placa))
            {
                _logger.LogWarning("Placa duplicada: {Placa}", dto.Placa);
                return null;
            }

            var vehiculo = new Vehiculo
            {
                ID_Cliente        = dto.IdCliente,
                Placa             = dto.Placa.ToUpperInvariant(),
                Tipo              = dto.Tipo,
                Marca             = dto.Marca,
                Modelo            = dto.Modelo,
                Color             = dto.Color,
                Capacidad         = dto.CapacidadTon,
                AnoFabricacion    = dto.AnoFabricacion,
                PesoTara          = dto.PesoTaraKg,
                VIN               = dto.VIN,
                Observaciones     = dto.Observaciones,
                Estado            = "ACTIVO",
                FechaRegistro     = DateTime.Now,
                FechaModificacion = DateTime.Now
            };

            await _vehiculoRepository.AgregarAsync(vehiculo);
            _logger.LogInformation("Vehículo creado: {Placa}", vehiculo.Placa);
            return vehiculo.ID_Vehiculo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CrearAsync vehiculo {Placa}", dto.Placa);
            return null;
        }
    }

    public async Task<bool> ActualizarAsync(int idVehiculo, CrearActualizarVehiculoDTO dto)
    {
        try
        {
            var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(idVehiculo);
            if (vehiculo == null) return false;

            vehiculo.Tipo             = dto.Tipo;
            vehiculo.Marca            = dto.Marca;
            vehiculo.Modelo           = dto.Modelo;
            vehiculo.Color            = dto.Color;
            vehiculo.Capacidad        = dto.CapacidadTon;
            vehiculo.AnoFabricacion   = dto.AnoFabricacion;
            vehiculo.PesoTara         = dto.PesoTaraKg;
            vehiculo.VIN              = dto.VIN;
            vehiculo.Observaciones    = dto.Observaciones;
            vehiculo.FechaModificacion = DateTime.Now;

            await _vehiculoRepository.ActualizarAsync(vehiculo);
            _logger.LogInformation("Vehículo actualizado: {Id}", idVehiculo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ActualizarAsync vehiculo {Id}", idVehiculo);
            return false;
        }
    }

    public async Task<bool> CambiarEstadoAsync(int idVehiculo, string nuevoEstado)
    {
        try
        {
            var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(idVehiculo);
            if (vehiculo == null) return false;

            vehiculo.Estado           = nuevoEstado;
            vehiculo.FechaModificacion = DateTime.Now;
            await _vehiculoRepository.ActualizarAsync(vehiculo);
            _logger.LogInformation("Estado vehículo {Id} → {Estado}", idVehiculo, nuevoEstado);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CambiarEstadoAsync {Id}", idVehiculo);
            return false;
        }
    }

    private async Task<string> ObtenerNombreClienteAsync(int idCliente)
    {
        var c = await _clienteRepository.ObtenerPorIdAsync(idCliente);
        return c?.Nombre ?? "N/A";
    }

    private static VehiculoDTO MapearDTO(Vehiculo v, string clienteNombre) => new()
    {
        IdVehiculo       = v.ID_Vehiculo,
        IdCliente        = v.ID_Cliente,
        ClienteNombre    = clienteNombre,
        Placa            = v.Placa,
        Tipo             = v.Tipo,
        Marca            = v.Marca,
        Modelo           = v.Modelo,
        Color            = v.Color,
        CapacidadTon     = v.Capacidad,
        AnoFabricacion   = v.AnoFabricacion,
        PesoTaraKg       = v.PesoTara,
        VIN              = v.VIN,
        Estado           = v.Estado,
        UltimaPesada     = v.UltimaPesada,
        Observaciones    = v.Observaciones
    };
}
