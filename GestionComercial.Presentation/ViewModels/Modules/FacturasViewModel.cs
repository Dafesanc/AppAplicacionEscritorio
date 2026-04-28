using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class FacturasViewModel : ObservableObject
{
    private readonly IFacturaService _facturaService;

    [ObservableProperty] private ObservableCollection<FacturaDTO> _facturas = new();
    [ObservableProperty] private FacturaDTO? _facturaSeleccionada;
    [ObservableProperty] private bool _estaCargando;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private string? _mensajeExito;
    [ObservableProperty] private string _filtroEstado = "TODOS";
    [ObservableProperty] private string _filtro = string.Empty;
    [ObservableProperty] private decimal _totalPendiente;
    [ObservableProperty] private int _cantidadVencidas;

    // Formulario pago
    [ObservableProperty] private bool _mostrarPago;
    [ObservableProperty] private decimal _montoRecibido;

    // Modal detalle
    [ObservableProperty] private bool _mostrarDetalle;

    private List<FacturaDTO> _todasLasFacturas = new();

    public FacturasViewModel(IFacturaService facturaService)
    {
        _facturaService = facturaService;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            _todasLasFacturas = (await _facturaService.ObtenerPendientesPagoAsync()).ToList();
            var vencidas = await _facturaService.ObtenerVencidasAsync(30);
            CantidadVencidas = vencidas.Count();
            TotalPendiente = await _facturaService.ObtenerTotalPendientePagoAsync();
            AplicarFiltro();
        }
        catch
        {
            MensajeError = "Error al cargar facturas.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    partial void OnFiltroChanged(string value) => AplicarFiltro();
    partial void OnFiltroEstadoChanged(string value) => AplicarFiltro();

    private void AplicarFiltro()
    {
        var lista = _todasLasFacturas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Filtro))
            lista = lista.Where(f =>
                f.NumeroFactura.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                f.ClienteNombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase));

        if (FiltroEstado != "TODOS")
            lista = lista.Where(f => f.Estado == FiltroEstado);

        Facturas = new ObservableCollection<FacturaDTO>(lista);
    }

    [RelayCommand]
    private void RegistrarPago(FacturaDTO? factura)
    {
        if (factura == null) return;
        FacturaSeleccionada = factura;
        MontoRecibido = factura.MontoPendiente;
        MostrarPago = true;
    }

    [RelayCommand]
    private async Task ConfirmarPagoAsync()
    {
        if (FacturaSeleccionada == null || MontoRecibido <= 0)
        {
            MensajeError = "Ingrese un monto válido.";
            return;
        }

        EstaCargando = true;
        MensajeError = null;
        try
        {
            var ok = await _facturaService.MarcarPagadaAsync(FacturaSeleccionada.IdFactura, MontoRecibido);
            if (ok)
            {
                MensajeExito = $"Factura {FacturaSeleccionada.NumeroFactura} marcada como pagada.";
                MostrarPago = false;
                await CargarAsync();
            }
            else
            {
                MensajeError = "No se pudo registrar el pago.";
            }
        }
        catch { MensajeError = "Error al registrar el pago."; }
        finally { EstaCargando = false; }
    }

    [RelayCommand]
    private void CancelarPago() => MostrarPago = false;

    [RelayCommand]
    private void VerFactura(FacturaDTO? factura)
    {
        if (factura == null) return;
        FacturaSeleccionada = factura;
        MostrarDetalle = true;
    }

    [RelayCommand]
    private void CerrarDetalle() => MostrarDetalle = false;
}
