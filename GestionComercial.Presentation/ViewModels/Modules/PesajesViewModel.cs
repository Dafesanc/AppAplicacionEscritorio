using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class PesajesViewModel : ObservableObject
{
    private readonly IPesajeService _pesajeService;
    private readonly IClienteService _clienteService;

    [ObservableProperty] private ObservableCollection<PesajeDTO> _pesajes = new();
    [ObservableProperty] private ObservableCollection<ClienteDTO> _clientes = new();
    [ObservableProperty] private PesajeDTO? _pesajeSeleccionado;
    [ObservableProperty] private bool _estaCargando;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private string? _mensajeExito;

    // Panel registro manual
    [ObservableProperty] private bool _mostrarRegistro;
    [ObservableProperty] private int _vehiculoIdManual;
    [ObservableProperty] private string _placaManual = string.Empty;
    [ObservableProperty] private decimal _pesoManual;
    [ObservableProperty] private string _tipoPesaje = "TARA";

    // Simulación báscula
    [ObservableProperty] private decimal _pesoBascula;
    [ObservableProperty] private bool _basculaConectada;
    [ObservableProperty] private string _estadoBascula = "Sin conexión";

    public PesajesViewModel(IPesajeService pesajeService, IClienteService clienteService)
    {
        _pesajeService = pesajeService;
        _clienteService = clienteService;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            var clientesLista = await _clienteService.ObtenerActivosAsync();
            Clientes = new ObservableCollection<ClienteDTO>(clientesLista);
        }
        catch
        {
            MensajeError = "Error al cargar datos.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task CargarHistorialVehiculoAsync(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa)) return;
        EstaCargando = true;
        MensajeError = null;
        try
        {
            // Busca por el vehiculo ID a partir de la placa — usa el primer pesaje disponible como referencia
            // En producción se buscaría en el repositorio de vehículos directamente
            MensajeError = "Ingrese el ID del vehículo para ver historial.";
        }
        finally { EstaCargando = false; }
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void AbrirRegistroManual()
    {
        VehiculoIdManual = 0;
        PlacaManual = string.Empty;
        PesoManual = 0;
        TipoPesaje = "TARA";
        MensajeError = null;
        MostrarRegistro = true;
    }

    [RelayCommand]
    private async Task RegistrarPesajeManualAsync()
    {
        if (VehiculoIdManual <= 0 || PesoManual <= 0)
        {
            MensajeError = "Ingrese ID de vehículo y peso válidos.";
            return;
        }

        EstaCargando = true;
        MensajeError = null;
        try
        {
            var dto = new RegistrarPesajeDTO
            {
                IdVehiculo = VehiculoIdManual,
                Tipo = TipoPesaje,
                PesoKg = PesoManual,
                EstadoBascula = "MANUAL"
            };

            var id = await _pesajeService.RegistrarPesajeAsync(dto);
            if (id != null)
            {
                MensajeExito = $"Pesaje registrado: {PesoManual:N2} kg ({TipoPesaje}).";
                MostrarRegistro = false;
                var pesaje = await _pesajeService.ObtenerPorIdAsync(id.Value);
                if (pesaje != null)
                    Pesajes.Insert(0, pesaje);
            }
            else
            {
                MensajeError = "No se pudo registrar el pesaje.";
            }
        }
        catch { MensajeError = "Error al registrar pesaje."; }
        finally { EstaCargando = false; }
    }

    [RelayCommand]
    private void SimularLectura()
    {
        var random = new Random();
        PesoBascula = Math.Round((decimal)(random.NextDouble() * 20000 + 5000), 2);
        EstadoBascula = "Lectura estable";
        BasculaConectada = true;
        MensajeExito = $"Peso leído: {PesoBascula:N2} kg";
    }

    [RelayCommand]
    private void UsarPesoBascula()
    {
        if (PesoBascula > 0)
        {
            PesoManual = PesoBascula;
            MostrarRegistro = true;
        }
    }

    [RelayCommand]
    private void CancelarRegistro() => MostrarRegistro = false;
}
