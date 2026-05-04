using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.Services;
using GestionComercial.Presentation.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly IClienteService _clienteService;
    private readonly IInventarioService _inventarioService;
    private readonly IPesajeService _pesajeService;
    private readonly SessionService _session;

    [ObservableProperty] private int    _totalVentasHoy;
    [ObservableProperty] private decimal _montoVentasHoy;
    [ObservableProperty] private int    _totalClientesActivos;
    [ObservableProperty] private int    _pesajesHoy;
    [ObservableProperty] private int    _alertasStock;
    [ObservableProperty] private string _bienvenida = "Bienvenido";
    [ObservableProperty] private bool   _estaCargando;
    [ObservableProperty] private string? _mensajeError;

    public DashboardViewModel(
        IVentaService ventaService,
        IClienteService clienteService,
        IInventarioService inventarioService,
        IPesajeService pesajeService,
        SessionService session)
    {
        _ventaService      = ventaService;
        _clienteService    = clienteService;
        _inventarioService = inventarioService;
        _pesajeService     = pesajeService;
        _session           = session;

        var nombre = _session.UsuarioActual?.NombreCompleto ?? "Usuario";
        var hora   = DateTime.Now.Hour;
        var saludo = hora < 12 ? "Buenos días" : hora < 18 ? "Buenas tardes" : "Buenas noches";
        Bienvenida = $"{saludo}, {nombre}";
    }

    [RelayCommand]
    public async Task CargarDatosAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            var hoy    = DateTime.Today;
            var manana = hoy.AddDays(1);

            var ventas   = (await _ventaService.ObtenerPorFechaAsync(hoy, manana)).ToList();
            var clientes = (await _clienteService.ObtenerActivosAsync()).ToList();
            var stock    = (await _inventarioService.ObtenerStockBajoAsync()).ToList();
            var pesajes  = (await _pesajeService.ObtenerPorFechaAsync(hoy, manana)).ToList();

            TotalVentasHoy       = ventas.Count;
            MontoVentasHoy       = ventas.Sum(v => v.TotalVenta);
            TotalClientesActivos = clientes.Count;
            AlertasStock         = stock.Count;
            PesajesHoy           = pesajes.Count;
        }
        catch
        {
            MensajeError = "No se pudieron cargar los datos del dashboard.";
        }
        finally
        {
            EstaCargando = false;
        }
    }
}
