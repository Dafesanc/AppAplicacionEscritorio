namespace GestionComercial.Application.Services;

using GestionComercial.Application.DTOs;
using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.Extensions.Logging;

public class PesajeService : IPesajeService
{
    private readonly IRepository<Pesaje> _pesajeRepository;
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly ILogger<PesajeService> _logger;

    public PesajeService(
        IRepository<Pesaje> pesajeRepository,
        IVehiculoRepository vehiculoRepository,
        ILogger<PesajeService> logger)
    {
        _pesajeRepository = pesajeRepository;
        _vehiculoRepository = vehiculoRepository;
        _logger = logger;
    }

    public async Task<PesajeDTO?> ObtenerPorIdAsync(int idPesaje)
    {
        try
        {
            var pesaje = await _pesajeRepository.ObtenerPorIdAsync(idPesaje);
            if (pesaje == null) return null;

            var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(pesaje.ID_Vehiculo);
            return MapearDTO(pesaje, vehiculo?.Placa ?? "N/A");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorIdAsync");
            return null;
        }
    }

    public async Task<PesajeDTO?> ObtenerUltimoPesajeAsync(int idVehiculo)
    {
        try
        {
            var pesajes = await _pesajeRepository.ObtenerTodosAsync();
            var ultimo = pesajes
                .Where(p => p.ID_Vehiculo == idVehiculo)
                .OrderByDescending(p => p.FechaPesaje)
                .FirstOrDefault();

            if (ultimo == null) return null;
            var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(idVehiculo);
            return MapearDTO(ultimo, vehiculo?.Placa ?? "N/A");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerUltimoPesajeAsync");
            return null;
        }
    }

    public async Task<IEnumerable<PesajeDTO>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            var pesajes = await _pesajeRepository.ObtenerTodosAsync();
            var result = pesajes.Where(p => p.FechaPesaje >= desde && p.FechaPesaje <= hasta).ToList();

            var placas = new Dictionary<int, string>();
            foreach (var p in result)
            {
                if (!placas.ContainsKey(p.ID_Vehiculo))
                {
                    var v = await _vehiculoRepository.ObtenerPorIdAsync(p.ID_Vehiculo);
                    placas[p.ID_Vehiculo] = v?.Placa ?? "N/A";
                }
            }

            return result.Select(p => MapearDTO(p, placas[p.ID_Vehiculo]));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorFechaAsync");
            return Enumerable.Empty<PesajeDTO>();
        }
    }

    public async Task<IEnumerable<PesajeDTO>> ObtenerHistorialVehiculoAsync(int idVehiculo, int limitUltimos = 10)
    {
        try
        {
            var pesajes = await _pesajeRepository.ObtenerTodosAsync();
            var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(idVehiculo);
            var placa = vehiculo?.Placa ?? "N/A";

            return pesajes
                .Where(p => p.ID_Vehiculo == idVehiculo)
                .OrderByDescending(p => p.FechaPesaje)
                .Take(limitUltimos)
                .Select(p => MapearDTO(p, placa));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerHistorialVehiculoAsync");
            return Enumerable.Empty<PesajeDTO>();
        }
    }

    public async Task<int?> RegistrarPesajeAsync(RegistrarPesajeDTO pesajeDTO)
    {
        try
        {
            var pesaje = new Pesaje
            {
                ID_Vehiculo = pesajeDTO.IdVehiculo,
                TipoPesaje = pesajeDTO.Tipo,
                PesoKg = pesajeDTO.PesoKg,
                Temperatura = pesajeDTO.Temperatura,
                Humedad = pesajeDTO.Humedad,
                EstadoBascula = pesajeDTO.EstadoBascula,
                FechaPesaje = DateTime.Now,
                UsuarioPesaje = 0
            };

            await _pesajeRepository.AgregarAsync(pesaje);
            _logger.LogInformation("Pesaje registrado: vehículo {Id}, tipo {Tipo}, {Peso} kg", pesajeDTO.IdVehiculo, pesajeDTO.Tipo, pesajeDTO.PesoKg);
            return pesaje.ID_Pesaje;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en RegistrarPesajeAsync");
            return null;
        }
    }

    public async Task<(decimal PesoNeto, PesajeDTO? Tara, PesajeDTO? Bruto)?> ObtenerPesoNetoAsync(int idTara, int idBruto)
    {
        try
        {
            var tara = await ObtenerPorIdAsync(idTara);
            var bruto = await ObtenerPorIdAsync(idBruto);

            if (tara == null || bruto == null) return null;

            var pesoNeto = bruto.PesoKg - tara.PesoKg;
            return (pesoNeto, tara, bruto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPesoNetoAsync");
            return null;
        }
    }

    public Task ConectarBasculaAsync(string puerto, int baudRate = 9600)
    {
        _logger.LogInformation("Simulando conexión báscula: {Puerto} @ {BaudRate}", puerto, baudRate);
        return Task.CompletedTask;
    }

    public Task DesconectarBasculaAsync()
    {
        _logger.LogInformation("Simulando desconexión báscula");
        return Task.CompletedTask;
    }

    private static PesajeDTO MapearDTO(Pesaje p, string placa) => new()
    {
        IdPesaje = p.ID_Pesaje,
        IdVehiculo = p.ID_Vehiculo,
        VehiculoPlaca = placa,
        Tipo = p.TipoPesaje,
        PesoKg = p.PesoKg,
        FechaPesaje = p.FechaPesaje,
        ObservacionesBascula = p.EstadoBascula
    };
}
