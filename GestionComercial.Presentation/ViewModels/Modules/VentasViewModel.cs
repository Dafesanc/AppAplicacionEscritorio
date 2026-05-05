using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;
using GestionComercial.Presentation.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class VentasViewModel : ObservableObject
{
    private readonly IVentaService      _ventaService;
    private readonly IClienteService    _clienteService;
    private readonly IVehiculoService   _vehiculoService;
    private readonly IInventarioService _inventarioService;
    private readonly SessionService     _sessionService;

    // ── Lista principal ──────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<VentaDTO> _ventas = new();
    [ObservableProperty] private VentaDTO? _ventaSeleccionada;
    [ObservableProperty] private bool      _estaCargando;
    [ObservableProperty] private string?   _mensajeError;
    [ObservableProperty] private string?   _mensajeExito;
    [ObservableProperty] private DateTime  _filtroDesde = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime  _filtroHasta = DateTime.Today;
    [ObservableProperty] private string    _filtroCliente = string.Empty;
    [ObservableProperty] private string    _filtroEstado  = "TODOS";
    [ObservableProperty] private decimal   _totalMostrado;
    [ObservableProperty] private int       _cantidadMostrada;

    // ── Formulario nueva venta ───────────────────────────────────────────────
    [ObservableProperty] private bool _mostrarFormulario;

    // Fuentes de datos para combos
    [ObservableProperty] private ObservableCollection<ClienteDTO>   _formClientes   = new();
    [ObservableProperty] private ObservableCollection<VehiculoDTO>  _formVehiculos  = new();
    [ObservableProperty] private ObservableCollection<ProductoDTO>  _formProductos  = new();

    // Selecciones del usuario
    [ObservableProperty] private int    _formIdCliente;
    [ObservableProperty] private int    _formIdVehiculo;
    [ObservableProperty] private int    _formIdProducto;

    // Campos derivados (read-only en la UI)
    [ObservableProperty] private string _formDescuentoPct      = "0.00 %";
    [ObservableProperty] private string _formPesoTaraVehiculo  = string.Empty;

    // Campos editables
    [ObservableProperty] private string _formUnidadMedida  = "Kg";
    [ObservableProperty] private string _formPesoNeto      = string.Empty;
    [ObservableProperty] private string _formTotal         = string.Empty;
    [ObservableProperty] private string _formTipoDocumento = "TICKET";

    public string[] EstadosFiltro  { get; } = ["TODOS", "BORRADOR", "COMPLETADA", "ANULADA"];
    public string[] TiposDocumento { get; } = ["TICKET", "FACTURA"];
    public string[] UnidadesMedida { get; } = ["Kg", "Unidad"];

    private List<VentaDTO> _todasLasVentas = [];

    public VentasViewModel(
        IVentaService      ventaService,
        IClienteService    clienteService,
        IVehiculoService   vehiculoService,
        IInventarioService inventarioService,
        SessionService     sessionService)
    {
        _ventaService      = ventaService;
        _clienteService    = clienteService;
        _vehiculoService   = vehiculoService;
        _inventarioService = inventarioService;
        _sessionService    = sessionService;
    }

    // ── Filtros ──────────────────────────────────────────────────────────────

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

    // ── Completar venta ──────────────────────────────────────────────────────

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
                MensajeExito = "Venta completada y stock actualizado.";
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

    // ── Nueva venta ──────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task NuevaVentaAsync()
    {
        MensajeError = null;
        MensajeExito = null;
        LimpiarFormulario();
        EstaCargando = true;
        try
        {
            // Secuencial: el DbContext es singleton en WPF; operaciones paralelas
            // sobre el mismo contexto lanzan InvalidOperationException.
            FormClientes  = new ObservableCollection<ClienteDTO>(await _clienteService.ObtenerActivosAsync());
            FormProductos = new ObservableCollection<ProductoDTO>(await _inventarioService.ObtenerActivosAsync());
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

    // ── Handlers reactivos del formulario ────────────────────────────────────

    partial void OnFormIdClienteChanged(int value)
    {
        _ = CargarVehiculosYDescuentoAsync(value);
    }

    private async Task CargarVehiculosYDescuentoAsync(int idCliente)
    {
        FormIdVehiculo       = 0;
        FormVehiculos        = new ObservableCollection<VehiculoDTO>();
        FormPesoTaraVehiculo = string.Empty;
        FormDescuentoPct     = "0.00 %";

        if (idCliente <= 0) return;

        try
        {
            var vehiculos = await _vehiculoService.ObtenerPorClienteAsync(idCliente);
            FormVehiculos = new ObservableCollection<VehiculoDTO>(vehiculos);

            var cliente = FormClientes.FirstOrDefault(c => c.IdCliente == idCliente);
            if (cliente != null)
                FormDescuentoPct = $"{cliente.DescuentoPorDefecto:N2} %";
        }
        catch { /* no interrumpir la UI */ }
    }

    partial void OnFormIdVehiculoChanged(int value)
    {
        var vehiculo = FormVehiculos.FirstOrDefault(v => v.IdVehiculo == value);
        FormPesoTaraVehiculo = vehiculo?.PesoTaraKg.HasValue == true
            ? $"{vehiculo.PesoTaraKg.Value:N2} kg"
            : string.Empty;
    }

    partial void OnFormIdProductoChanged(int value)
    {
        var producto = FormProductos.FirstOrDefault(p => p.IdProducto == value);
        if (producto == null) return;

        FormUnidadMedida = producto.Unidad is "Kg" or "Tonelada" ? "Kg" : "Unidad";
    }

    // ── Guardar ──────────────────────────────────────────────────────────────

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
        //AQUI SUPONGO QUE DEBERIA COMENZAR  A INCLUIR LOS CAMBIOS PARA OBTENER DESCUENTO
        if (!decimal.TryParse(FormTotal, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture, out var total) || total <= 0)
        {
            MensajeError = "El total debe ser un valor numérico mayor a cero.";
            return;
        }

        var cliente = FormClientes.FirstOrDefault(c => c.IdCliente == FormIdCliente);
        var descuento = cliente?.DescuentoPorDefecto ?? 0;

        decimal? pesoNeto = decimal.TryParse(FormPesoNeto, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var pn) ? pn : null;

        var vehiculo = FormVehiculos.FirstOrDefault(v => v.IdVehiculo == FormIdVehiculo);
        decimal? pesoTara = vehiculo?.PesoTaraKg;

        EstaCargando = true;
        try
        {
            var dto = new CrearVentaManualDTO
            {
                IdCliente     = FormIdCliente,
                IdVehiculo    = FormIdVehiculo > 0 ? FormIdVehiculo : null,
                IdProducto    = FormIdProducto > 0 ? FormIdProducto : null,
                PesoNetoKg    = pesoNeto,
                PesoTaraKg    = pesoTara,
                Cantidad      = pesoNeto ?? 0,
                Total         = total,
                Descuento     = descuento,
                TipoDocumento = FormTipoDocumento,
                UsuarioId     = _sessionService.UsuarioActual?.ID_Usuario ?? 0
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
        FormIdCliente        = 0;
        FormIdVehiculo       = 0;
        FormIdProducto       = 0;
        FormVehiculos        = new ObservableCollection<VehiculoDTO>();
        FormProductos        = new ObservableCollection<ProductoDTO>();
        FormDescuentoPct     = "0.00 %";
        FormPesoTaraVehiculo = string.Empty;
        FormUnidadMedida     = "Kg";
        FormPesoNeto         = string.Empty;
        FormTotal            = string.Empty;
        FormTipoDocumento    = "TICKET";
    }
}
