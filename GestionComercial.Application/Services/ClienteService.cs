namespace GestionComercial.Application.Services;

using GestionComercial.Application.DTOs;
using GestionComercial.Application.Interfaces;
using GestionComercial.Domain.Entities;
using Microsoft.Extensions.Logging;

/// <summary>
/// Servicio para gestión de Clientes
/// </summary>
public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(IClienteRepository clienteRepository, ILogger<ClienteService> logger)
    {
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    public async Task<ClienteDTO?> ObtenerPorIdAsync(int idCliente)
    {
        try
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(idCliente);
            return cliente == null ? null : MapearDTO(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorIdAsync");
            return null;
        }
    }

    public async Task<ClienteDTO?> ObtenerPorIdentificacionAsync(string numeroIdentificacion)
    {
        try
        {
            var cliente = await _clienteRepository.ObtenerPorIdentificacionAsync(numeroIdentificacion);
            return cliente == null ? null : MapearDTO(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPorIdentificacionAsync");
            return null;
        }
    }

    public async Task<IEnumerable<ClienteDTO>> ObtenerActivosAsync()
    {
        try
        {
            var clientes = await _clienteRepository.ObtenerActivosAsync();
            return clientes.Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerActivosAsync");
            return Enumerable.Empty<ClienteDTO>();
        }
    }

    public async Task<IEnumerable<ClienteDTO>> ObtenerClientesFrecuentesAsync(int top = 10)
    {
        try
        {
            var clientes = await _clienteRepository.ObtenerClientesFrecuentesAsync(top);
            return clientes.Select(MapearDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerClientesFrecuentesAsync");
            return Enumerable.Empty<ClienteDTO>();
        }
    }

    public async Task<decimal> ObtenerCreditoDisponibleAsync(int idCliente)
    {
        try
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(idCliente);
            return cliente?.CreditoDisponible ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerCreditoDisponibleAsync");
            return 0;
        }
    }

    public async Task<int?> CrearAsync(CrearActualizarClienteDTO clienteDTO)
    {
        try
        {
            if (await _clienteRepository.ExisteIdentificacionAsync(clienteDTO.NumeroIdentificacion))
            {
                _logger.LogWarning("Identificación duplicada: {Identificacion}", clienteDTO.NumeroIdentificacion);
                return null;
            }

            var cliente = new Cliente
            {
                CodigoCliente = $"CLI-{DateTime.Now:yyyyMMddHHmmss}",
                Nombre = clienteDTO.Nombre,
                TipoIdentificacion = clienteDTO.TipoIdentificacion,
                NumeroIdentificacion = clienteDTO.NumeroIdentificacion,
                Categoria = clienteDTO.Categoria,
                Email = clienteDTO.Email,
                Contacto = clienteDTO.Telefono,
                Direccion = clienteDTO.Direccion,
                DescuentoPorDefecto = clienteDTO.DescuentoPorDefecto,
                PlazoCredito = clienteDTO.PlazoCredito,
                LimiteCredito = clienteDTO.LimiteCredito,
                Estado = "ACTIVO",
                Observaciones = clienteDTO.Observaciones,
                FechaRegistro = DateTime.Now,
                FechaModificacion = DateTime.Now
            };

            await _clienteRepository.AgregarAsync(cliente);
            _logger.LogInformation("Cliente creado: {Nombre}", clienteDTO.Nombre);
            return cliente.ID_Cliente;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CrearAsync");
            return null;
        }
    }

    public async Task<bool> ActualizarAsync(int idCliente, CrearActualizarClienteDTO clienteDTO)
    {
        try
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(idCliente);
            if (cliente == null) return false;

            cliente.Nombre = clienteDTO.Nombre;
            cliente.Email = clienteDTO.Email;
            cliente.Contacto = clienteDTO.Telefono;
            cliente.Direccion = clienteDTO.Direccion;
            cliente.DescuentoPorDefecto = clienteDTO.DescuentoPorDefecto;
            cliente.PlazoCredito = clienteDTO.PlazoCredito;
            cliente.LimiteCredito = clienteDTO.LimiteCredito;
            cliente.Observaciones = clienteDTO.Observaciones;
            cliente.FechaModificacion = DateTime.Now;

            await _clienteRepository.ActualizarAsync(cliente);
            _logger.LogInformation("Cliente actualizado: {Id}", idCliente);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ActualizarAsync");
            return false;
        }
    }

    public async Task<bool> BloquearAsync(int idCliente)
    {
        try
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(idCliente);
            if (cliente == null) return false;

            cliente.Estado = "BLOQUEADO";
            cliente.FechaModificacion = DateTime.Now;

            await _clienteRepository.ActualizarAsync(cliente);
            _logger.LogInformation("Cliente bloqueado: {Id}", idCliente);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en BloquearAsync");
            return false;
        }
    }

    public async Task<bool> DesbloquearAsync(int idCliente)
    {
        try
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(idCliente);
            if (cliente == null) return false;

            cliente.Estado = "ACTIVO";
            cliente.FechaModificacion = DateTime.Now;

            await _clienteRepository.ActualizarAsync(cliente);
            _logger.LogInformation("Cliente desbloqueado: {Id}", idCliente);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en DesbloquearAsync");
            return false;
        }
    }

    private static ClienteDTO MapearDTO(Cliente cliente) => new()
    {
        IdCliente = cliente.ID_Cliente,
        CodigoCliente = cliente.CodigoCliente,
        Nombre = cliente.Nombre,
        TipoIdentificacion = cliente.TipoIdentificacion,
        NumeroIdentificacion = cliente.NumeroIdentificacion,
        Categoria = cliente.Categoria,
        Email = cliente.Email,
        Telefono = cliente.Contacto,
        Direccion = cliente.Direccion,
        DescuentoPorDefecto = cliente.DescuentoPorDefecto,
        PlazoCredito = cliente.PlazoCredito,
        LimiteCredito = cliente.LimiteCredito,
        SaldoCredito = cliente.SaldoCredito,
        Estado = cliente.Estado,
        Observaciones = cliente.Observaciones
    };
}
