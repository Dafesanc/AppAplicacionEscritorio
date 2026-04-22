using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class VentasViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly IClienteService _clienteService;

    [ObservableProperty] private ObservableCollection<VentaDTO> _ventas = new();
    [ObservableProperty] private VentaDTO? _ventaSeleccionada;
    [ObservableProperty] private bool _estaCargando;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private string? _mensajeExito;
    [ObservableProperty] private DateTime _filtroDesde = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _filtroHasta = DateTime.Today;
    [ObservableProperty] private string _filtroCliente = string.Empty;
    [ObservableProperty] private string _filtroEstado = "TODOS";
    [ObservableProperty] private decimal _totalMostrado;
    [ObservableProperty] private int _cantidadMostrada;

    private List<VentaDTO> _todasLasVentas = new();

    public VentasViewModel(IVentaService ventaService, IClienteService clienteService)
    {
        _ventaService = ventaService;
        _clienteService = clienteService;
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
    partial void OnFiltroEstadoChanged(string value) => AplicarFiltros();

    private void AplicarFiltros()
    {
        var lista = _todasLasVentas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FiltroCliente))
            lista = lista.Where(v => v.ClienteNombre.Contains(FiltroCliente, StringComparison.OrdinalIgnoreCase)
                                  || v.NumeroVenta.Contains(FiltroCliente, StringComparison.OrdinalIgnoreCase));

        if (FiltroEstado != "TODOS")
            lista = lista.Where(v => v.Estado == FiltroEstado);

        var result = lista.ToList();
        Ventas = new ObservableCollection<VentaDTO>(result);
        TotalMostrado = result.Sum(v => v.TotalVenta);
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
}
