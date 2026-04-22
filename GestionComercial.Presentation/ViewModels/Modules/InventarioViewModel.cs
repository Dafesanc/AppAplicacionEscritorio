using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class InventarioViewModel : ObservableObject
{
    private readonly IInventarioService _inventarioService;

    [ObservableProperty] private ObservableCollection<ProductoDTO> _productos = new();
    [ObservableProperty] private ProductoDTO? _productoSeleccionado;
    [ObservableProperty] private ObservableCollection<MovimientoInventarioDTO> _movimientos = new();
    [ObservableProperty] private bool _estaCargando;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private string? _mensajeExito;
    [ObservableProperty] private string _filtro = string.Empty;
    [ObservableProperty] private bool _soloStockBajo;
    [ObservableProperty] private decimal _valorTotalInventario;
    [ObservableProperty] private int _productosConStockBajo;

    // Formulario ajuste
    [ObservableProperty] private bool _mostrarAjuste;
    [ObservableProperty] private decimal _cantidadAjuste;
    [ObservableProperty] private string _razonAjuste = string.Empty;

    private List<ProductoDTO> _todosLosProductos = new();

    public InventarioViewModel(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            _todosLosProductos = (await _inventarioService.ObtenerActivosAsync()).ToList();
            ValorTotalInventario = await _inventarioService.ObtenerValorInventarioTotalAsync();
            ProductosConStockBajo = _todosLosProductos.Count(p => p.Stock < p.StockMinimo);
            AplicarFiltro();
        }
        catch
        {
            MensajeError = "Error al cargar inventario.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    partial void OnFiltroChanged(string value) => AplicarFiltro();
    partial void OnSoloStockBajoChanged(bool value) => AplicarFiltro();

    private void AplicarFiltro()
    {
        var lista = _todosLosProductos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Filtro))
            lista = lista.Where(p =>
                p.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                p.Codigo.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                p.TipoMaterial.Contains(Filtro, StringComparison.OrdinalIgnoreCase));

        if (SoloStockBajo)
            lista = lista.Where(p => p.Stock < p.StockMinimo);

        Productos = new ObservableCollection<ProductoDTO>(lista);
    }

    [RelayCommand]
    private async Task SeleccionarProductoAsync(ProductoDTO? producto)
    {
        if (producto == null) return;
        ProductoSeleccionado = producto;
        EstaCargando = true;
        try
        {
            var movs = await _inventarioService.ObtenerMovimientosAsync(producto.IdProducto, 90);
            Movimientos = new ObservableCollection<MovimientoInventarioDTO>(movs);
        }
        catch { Movimientos = new ObservableCollection<MovimientoInventarioDTO>(); }
        finally { EstaCargando = false; }
    }

    [RelayCommand]
    private void AbrirAjuste(ProductoDTO? producto)
    {
        if (producto == null) return;
        ProductoSeleccionado = producto;
        CantidadAjuste = 0;
        RazonAjuste = string.Empty;
        MostrarAjuste = true;
    }

    [RelayCommand]
    private async Task AplicarAjusteAsync()
    {
        if (ProductoSeleccionado == null || CantidadAjuste == 0)
        {
            MensajeError = "Ingrese una cantidad y producto válidos.";
            return;
        }

        EstaCargando = true;
        MensajeError = null;
        try
        {
            var ok = await _inventarioService.AjustarStockAsync(
                ProductoSeleccionado.IdProducto, CantidadAjuste, RazonAjuste);

            if (ok)
            {
                MensajeExito = $"Stock ajustado: {CantidadAjuste:+0.##;-0.##} unidades.";
                MostrarAjuste = false;
                await CargarAsync();
            }
            else
            {
                MensajeError = "No se pudo aplicar el ajuste.";
            }
        }
        catch { MensajeError = "Error al ajustar stock."; }
        finally { EstaCargando = false; }
    }

    [RelayCommand]
    private void CancelarAjuste() => MostrarAjuste = false;
}
