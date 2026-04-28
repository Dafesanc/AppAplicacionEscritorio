using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;
using GestionComercial.Presentation.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class VentasViewModel : ObservableObject
{
    private readonly IVentaService    _ventaService;
    private readonly IClienteService  _clienteService;
    private readonly IVehiculoService _vehiculoService;
    private readonly SessionService   _sessionService;

    [ObservableProperty] private ObservableCollection<VentaDTO> _ventas = new();
    [ObservableProperty] private VentaDTO? _ventaSeleccionada;
    [ObservableProperty] private bool  _estaCargando;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private string? _mensajeExito;
    [ObservableProperty] private DateTime _filtroDesde = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _filtroHasta = DateTime.Today;
    [ObservableProperty] private string   _filtroCliente = string.Empty;
    [ObservableProperty] private string   _filtroEstado  = "TODOS";
    [ObservableProperty] private decimal  _totalMostrado;
    [ObservableProperty] private int      _cantidadMostrada;

    // --- Formulario nueva venta ---
    [ObservableProperty] private bool _mostrarFormulario;
    [ObservableProperty] private ObservableCollection<ClienteDTO>  _formClientes  = new();
    [ObservableProperty] private ObservableCollection<VehiculoDTO> _formVehiculos = new();
    [ObservableProperty] private int    _formIdCliente;
    [ObservableProperty] private int    _formIdVehiculo;
    [ObservableProperty] private string _formPesoNeto      = string.Empty;
    [ObservableProperty] private string _formTotal         = string.Empty;
    [ObservableProperty] private string _formDescuento     = "0";
    [ObservableProperty] private string _formTipoDocumento = "TICKET";

    public string[] EstadosFiltro  { get; } = { "TODOS", "BORRADOR", "COMPLETADA", "ANULADA" };
    public string[] TiposDocumento { get; } = { "TICKET", "FACTURA" };

    private List<VentaDTO> _todasLasVentas = new();

    public VentasViewModel(
        IVentaService    ventaService,
        IClienteService  clienteService,
        IVehiculoService vehiculoService,
        SessionService   sessionService)
    {
        _ventaService    = ventaService;
        _clienteService  = clienteService;
        _vehiculoService = vehiculoService;
        _sessionService  = sessionService;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            _todasLasVentas = (await _ventaService.ObtenerPorFechaAsync(FiltroDesde, FiltroHasta.AddDays(1))).ToList();
            AplicarFiltros();
        }
        catch
        {
            MensajeError = "Error al cargar ventas.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    partial void OnFiltroClienteChanged(string value) => AplicarFiltros();
    partial void OnFiltroEstadoChanged(string value)  => AplicarFiltros();

    private void AplicarFiltros()
    {
        var lista = _todasLasVentas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FiltroCliente))
            lista = lista.Where(v =>
                v.ClienteNombre.Contains(FiltroCliente, StringComparison.OrdinalIgnoreCase) ||
                v.NumeroVenta.Contains(FiltroCliente, StringComparison.OrdinalIgnoreCase));

        if (FiltroEstado != "TODOS")
            lista = lista.Where(v => v.Estado == FiltroEstado);

        var result = lista.ToList();
        Ventas           = new ObservableCollection<VentaDTO>(result);
        TotalMostrado    = result.Sum(v => v.TotalVenta);
        CantidadMostrada = result.Count;
    }

    [RelayCommand]
    private async Task BuscarAsync() => await CargarAsync();

    [RelayCommand]
    private async Task CompletarVentaAsync(VentaDTO? venta)
    {
        if (venta == null) return;
        EstaCargando = true;
        MensajeError = null;
        try
        {
            var ok = await _ventaService.MarkupSaleComplete(venta.IdVenta);
            if (ok)
            {
                MensajeExito = "Venta completada.";
                await CargarAsync();
            }
            else
            {
                MensajeError = "No se pudo completar la venta.";
            }
        }
        catch { MensajeError = "Error al completar la venta."; }
        finally { EstaCargando = false; }
    }

    [RelayCommand]
    private async Task NuevaVentaAsync()
    {
        MensajeError  = null;
        MensajeExito  = null;
        LimpiarFormulario();
        EstaCargando = true;
        try
        {
            var tClientes  = _clienteService.ObtenerActivosAsync();
            var tVehiculos = _vehiculoService.ObtenerActivosAsync();
            await Task.WhenAll(tClientes, tVehiculos);
            FormClientes  = new ObservableCollection<ClienteDTO>(tClientes.Result);
            FormVehiculos = new ObservableCollection<VehiculoDTO>(tVehiculos.Result);
        }
        catch
        {
            MensajeError = "Error al cargar datos del formulario.";
        }
        finally
        {
            EstaCargando = false;
        }
        MostrarFormulario = true;
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormulario = false;
        LimpiarFormulario();
    }

    [RelayCommand]
    private async Task GuardarVentaAsync()
    {
        MensajeError = null;
        MensajeExito = null;

        if (FormIdCliente == 0)
        {
            MensajeError = "Debe seleccionar un cliente.";
            return;
        }
        if (!decimal.TryParse(FormTotal, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture, out var total) || total <= 0)
        {
            MensajeError = "El total debe ser un valor numérico mayor a cero.";
            return;
        }

        decimal.TryParse(FormDescuento, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var descuento);
        decimal? pesoNeto = decimal.TryParse(FormPesoNeto, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var pn) ? pn : null;

        EstaCargando = true;
        try
        {
            var dto = new CrearVentaManualDTO
            {
                IdCliente      = FormIdCliente,
                IdVehiculo     = FormIdVehiculo > 0 ? FormIdVehiculo : null,
                PesoNetoKg     = pesoNeto,
                Total          = total,
                Descuento      = descuento,
                TipoDocumento  = FormTipoDocumento,
                UsuarioId      = _sessionService.UsuarioActual?.ID_Usuario ?? 0
            };

            var id = await _ventaService.CrearManualAsync(dto);
            if (id != null)
            {
                MensajeExito      = "Venta registrada correctamente.";
                MostrarFormulario = false;
                await CargarAsync();
            }
            else
            {
                MensajeError = "No se pudo registrar la venta.";
            }
        }
        catch
        {
            MensajeError = "Error al guardar la venta.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private void LimpiarFormulario()
    {
        FormIdCliente      = 0;
        FormIdVehiculo     = 0;
        FormPesoNeto       = string.Empty;
        FormTotal          = string.Empty;
        FormDescuento      = "0";
        FormTipoDocumento  = "TICKET";
    }
}
