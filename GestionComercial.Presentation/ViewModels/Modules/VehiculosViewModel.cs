using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class VehiculosViewModel : ObservableObject
{
    private readonly IVehiculoService _vehiculoService;
    private readonly IClienteService  _clienteService;

    [ObservableProperty] private ObservableCollection<VehiculoDTO> _vehiculos = new();
    [ObservableProperty] private ObservableCollection<ClienteDTO>  _clientes  = new();
    [ObservableProperty] private VehiculoDTO?  _vehiculoSeleccionado;
    [ObservableProperty] private string        _filtroBusqueda = string.Empty;
    [ObservableProperty] private string        _filtroEstado   = "TODOS";
    [ObservableProperty] private bool          _estaCargando;
    [ObservableProperty] private string?       _mensajeError;
    [ObservableProperty] private string?       _mensajeExito;
    [ObservableProperty] private bool          _mostrarFormulario;

    // Campos del formulario
    [ObservableProperty] private int     _formIdVehiculo;
    [ObservableProperty] private int     _formIdCliente;
    [ObservableProperty] private string  _formPlaca       = string.Empty;
    [ObservableProperty] private string  _formTipo        = "Volqueta";
    [ObservableProperty] private string  _formMarca       = string.Empty;
    [ObservableProperty] private string  _formModelo      = string.Empty;
    [ObservableProperty] private string  _formColor       = string.Empty;
    [ObservableProperty] private string  _formCapacidad   = string.Empty;
    [ObservableProperty] private string  _formAno         = string.Empty;
    [ObservableProperty] private string  _formPesoTara    = string.Empty;
    [ObservableProperty] private string  _formVin         = string.Empty;
    [ObservableProperty] private string  _formObservaciones = string.Empty;
    [ObservableProperty] private bool    _esEdicion;

    private List<VehiculoDTO> _vehiculosTodos = new();

    public string TituloFormulario => EsEdicion ? "Editar Vehículo" : "Nuevo Vehículo";

    public string[] TiposVehiculo { get; } =
        { "Volqueta", "Gandola", "Furgón", "Camión", "Camioneta", "Otro" };

    public string[] EstadosFiltro { get; } =
        { "TODOS", "ACTIVO", "INACTIVO", "MANTENIMIENTO" };

    public VehiculosViewModel(IVehiculoService vehiculoService, IClienteService clienteService)
    {
        _vehiculoService = vehiculoService;
        _clienteService  = clienteService;
    }

    [RelayCommand]
    public async Task CargarDatosAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            _vehiculosTodos = (await _vehiculoService.ObtenerActivosAsync()).ToList();
            Clientes        = new ObservableCollection<ClienteDTO>(await _clienteService.ObtenerActivosAsync());
            AplicarFiltros();
        }
        catch
        {
            MensajeError = "No se pudieron cargar los vehículos.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    partial void OnFiltroBusquedaChanged(string value) => AplicarFiltros();
    partial void OnFiltroEstadoChanged(string value)   => AplicarFiltros();

    private void AplicarFiltros()
    {
        var resultado = _vehiculosTodos.AsEnumerable();

        if (FiltroEstado != "TODOS")
            resultado = resultado.Where(v => v.Estado == FiltroEstado);

        if (!string.IsNullOrWhiteSpace(FiltroBusqueda))
        {
            var texto = FiltroBusqueda.ToUpperInvariant();
            resultado = resultado.Where(v =>
                v.Placa.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                v.ClienteNombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                (v.Marca ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                (v.Modelo ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        Vehiculos = new ObservableCollection<VehiculoDTO>(resultado);
    }

    [RelayCommand]
    private void NuevoVehiculo()
    {
        LimpiarFormulario();
        EsEdicion = false;
        MostrarFormulario = true;
        MensajeExito = null;
        MensajeError = null;
    }

    [RelayCommand]
    private void EditarVehiculo(VehiculoDTO? vehiculo)
    {
        if (vehiculo == null) return;
        EsEdicion        = true;
        MostrarFormulario = true;
        MensajeExito     = null;
        MensajeError     = null;

        FormIdVehiculo    = vehiculo.IdVehiculo;
        FormIdCliente     = vehiculo.IdCliente;
        FormPlaca         = vehiculo.Placa;
        FormTipo          = vehiculo.Tipo;
        FormMarca         = vehiculo.Marca ?? string.Empty;
        FormModelo        = vehiculo.Modelo ?? string.Empty;
        FormColor         = vehiculo.Color ?? string.Empty;
        FormCapacidad     = vehiculo.CapacidadTon?.ToString("F2") ?? string.Empty;
        FormAno           = vehiculo.AnoFabricacion?.ToString() ?? string.Empty;
        FormPesoTara      = vehiculo.PesoTaraKg?.ToString("F2") ?? string.Empty;
        FormVin           = vehiculo.VIN ?? string.Empty;
        FormObservaciones = vehiculo.Observaciones ?? string.Empty;
        OnPropertyChanged(nameof(TituloFormulario));
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormulario = false;
        LimpiarFormulario();
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        MensajeError = null;
        if (string.IsNullOrWhiteSpace(FormPlaca) || FormIdCliente == 0 || string.IsNullOrWhiteSpace(FormTipo))
        {
            MensajeError = "Cliente, placa y tipo son obligatorios.";
            return;
        }

        EstaCargando = true;
        try
        {
            var dto = new CrearActualizarVehiculoDTO
            {
                IdCliente     = FormIdCliente,
                Placa         = FormPlaca.Trim().ToUpperInvariant(),
                Tipo          = FormTipo,
                Marca         = NullIfEmpty(FormMarca),
                Modelo        = NullIfEmpty(FormModelo),
                Color         = NullIfEmpty(FormColor),
                CapacidadTon  = decimal.TryParse(FormCapacidad, out var cap)  ? cap  : null,
                AnoFabricacion = int.TryParse(FormAno, out var ano)           ? ano  : null,
                PesoTaraKg    = decimal.TryParse(FormPesoTara, out var tara)  ? tara : null,
                VIN           = NullIfEmpty(FormVin),
                Observaciones = NullIfEmpty(FormObservaciones)
            };

            bool ok;
            if (EsEdicion)
                ok = await _vehiculoService.ActualizarAsync(FormIdVehiculo, dto);
            else
                ok = (await _vehiculoService.CrearAsync(dto)) != null;

            if (ok)
            {
                MensajeExito      = EsEdicion ? "Vehículo actualizado." : "Vehículo registrado.";
                MostrarFormulario = false;
                await CargarDatosAsync();
            }
            else
            {
                MensajeError = EsEdicion
                    ? "No se pudo actualizar el vehículo."
                    : "No se pudo registrar. Verifique que la placa no esté duplicada.";
            }
        }
        catch
        {
            MensajeError = "Error al guardar el vehículo.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync(VehiculoDTO? vehiculo)
    {
        if (vehiculo == null) return;
        var nuevoEstado = vehiculo.Estado == "ACTIVO" ? "INACTIVO" : "ACTIVO";
        EstaCargando = true;
        try
        {
            var ok = await _vehiculoService.CambiarEstadoAsync(vehiculo.IdVehiculo, nuevoEstado);
            if (ok)
            {
                MensajeExito = $"Estado cambiado a {nuevoEstado}.";
                await CargarDatosAsync();
            }
        }
        catch
        {
            MensajeError = "Error al cambiar el estado.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private void LimpiarFormulario()
    {
        FormIdVehiculo    = 0;
        FormIdCliente     = 0;
        FormPlaca         = string.Empty;
        FormTipo          = "Volqueta";
        FormMarca         = string.Empty;
        FormModelo        = string.Empty;
        FormColor         = string.Empty;
        FormCapacidad     = string.Empty;
        FormAno           = string.Empty;
        FormPesoTara      = string.Empty;
        FormVin           = string.Empty;
        FormObservaciones = string.Empty;
        OnPropertyChanged(nameof(TituloFormulario));
    }

    private static string? NullIfEmpty(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
