using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;
using GestionComercial.Presentation.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class InventarioViewModel : ObservableObject
{
    private readonly IInventarioService _inventarioService;
    private readonly SessionService     _session;

    // ── Lista principal ──────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<ProductoDTO> _productos = new();
    [ObservableProperty] private ProductoDTO? _productoSeleccionado;
    [ObservableProperty] private ObservableCollection<MovimientoInventarioDTO> _movimientos = new();
    [ObservableProperty] private bool    _estaCargando;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private string? _mensajeExito;
    [ObservableProperty] private string  _filtro = string.Empty;
    [ObservableProperty] private bool    _soloStockBajo;
    [ObservableProperty] private decimal _valorTotalInventario;
    [ObservableProperty] private int     _productosConStockBajo;

    // ── Modal Ajuste de Stock ────────────────────────────────────────────────
    [ObservableProperty] private bool    _mostrarAjuste;
    [ObservableProperty] private decimal _ajusteCantidad;
    [ObservableProperty] private string  _ajusteTipoMovimiento = "ENTRADA";
    [ObservableProperty] private string  _ajusteReferencia     = string.Empty;

    // ── Modal Nuevo Producto ─────────────────────────────────────────────────
    [ObservableProperty] private bool    _mostrarNuevoProducto;
    [ObservableProperty] private string? _mensajeErrorProducto;
    [ObservableProperty] private string  _formCodigo        = string.Empty;
    [ObservableProperty] private string  _formNombre        = string.Empty;
    [ObservableProperty] private string  _formTipoMaterial  = string.Empty;
    [ObservableProperty] private string  _formUnidad        = "Kg";
    [ObservableProperty] private string  _formPrecioBase    = string.Empty;
    [ObservableProperty] private string  _formStockInicial  = "0";
    [ObservableProperty] private string  _formStockMinimo   = "0";
    [ObservableProperty] private string  _formStockMaximo   = "0";
    [ObservableProperty] private string? _formDescripcion;
    [ObservableProperty] private string? _formProveedor;

    // ── Catálogos para ComboBoxes ────────────────────────────────────────────
    public string[] TiposMovimiento      { get; } = ["ENTRADA", "SALIDA"];
    public string[] ReferenciasMovimiento { get; } =
        ["INGRESO LOTE", "DEVOLUCIÓN", "AJUSTE MANUAL", "CADUCIDAD", "PÉRDIDA", "VENTA", "OTRO"];
    public string[] TiposMaterial  { get; } = ["Arena", "Piedra", "Cemento", "Grava", "Arcilla", "Otro"];
    public string[] UnidadesMedida { get; } = ["Kg", "Tonelada", "Unidad", "Metro", "Metro2", "Metro3"];

    private List<ProductoDTO> _todosLosProductos = [];

    public InventarioViewModel(IInventarioService inventarioService, SessionService session)
    {
        _inventarioService = inventarioService;
        _session           = session;
    }

    // ── Carga ────────────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task CargarAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            _todosLosProductos    = (await _inventarioService.ObtenerActivosAsync()).ToList();
            ValorTotalInventario  = await _inventarioService.ObtenerValorInventarioTotalAsync();
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

    partial void OnFiltroChanged(string value)      => AplicarFiltro();
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
        catch { Movimientos = []; }
        finally { EstaCargando = false; }
    }

    // ── Ajuste de Stock ──────────────────────────────────────────────────────

    [RelayCommand]
    private void AbrirAjuste(ProductoDTO? producto)
    {
        if (producto == null) return;
        ProductoSeleccionado  = producto;
        AjusteCantidad        = 0;
        AjusteTipoMovimiento  = TiposMovimiento[0];
        AjusteReferencia      = ReferenciasMovimiento[0];
        MensajeError          = null;
        MostrarAjuste         = true;
    }

    [RelayCommand]
    private async Task AplicarAjusteAsync()
    {
        if (ProductoSeleccionado == null || AjusteCantidad <= 0)
        {
            MensajeError = "Ingrese una cantidad mayor a cero.";
            return;
        }

        EstaCargando = true;
        MensajeError = null;
        try
        {
            var referencia = string.IsNullOrWhiteSpace(AjusteReferencia) ? "AJUSTE MANUAL" : AjusteReferencia;
            var ok = await _inventarioService.ActualizarStockAsync(
                ProductoSeleccionado.IdProducto,
                AjusteCantidad,
                AjusteTipoMovimiento,
                referencia);

            if (ok)
            {
                MensajeExito  = $"Stock actualizado: {AjusteTipoMovimiento} de {AjusteCantidad:N2} unidades.";
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
    private void CancelarAjuste()
    {
        MostrarAjuste = false;
        MensajeError  = null;
    }

    // ── Nuevo Producto ───────────────────────────────────────────────────────

    [RelayCommand]
    private void AbrirNuevoProducto()
    {
        FormCodigo           = string.Empty;
        FormNombre           = string.Empty;
        FormTipoMaterial     = TiposMaterial[0];
        FormUnidad           = UnidadesMedida[0];
        FormPrecioBase       = string.Empty;
        FormStockInicial     = "0";
        FormStockMinimo      = "0";
        FormStockMaximo      = "0";
        FormDescripcion      = null;
        FormProveedor        = null;
        MensajeErrorProducto = null;
        MostrarNuevoProducto = true;
    }

    [RelayCommand]
    private async Task GuardarProductoAsync()
    {
        MensajeErrorProducto = null;

        if (string.IsNullOrWhiteSpace(FormCodigo) || string.IsNullOrWhiteSpace(FormNombre))
        {
            MensajeErrorProducto = "Código y Nombre son obligatorios.";
            return;
        }
        if (!decimal.TryParse(FormPrecioBase,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture,
                out var precio) || precio < 0)
        {
            MensajeErrorProducto = "El precio debe ser un valor numérico válido.";
            return;
        }

        decimal.TryParse(FormStockInicial, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var stockInicial);
        decimal.TryParse(FormStockMinimo, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var stockMinimo);
        decimal.TryParse(FormStockMaximo, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var stockMaximo);

        EstaCargando = true;
        try
        {
            var dto = new CrearProductoDTO
            {
                Codigo       = FormCodigo.Trim().ToUpperInvariant(),
                Nombre       = FormNombre.Trim(),
                TipoMaterial = FormTipoMaterial,
                Unidad       = FormUnidad,
                PrecioBase   = precio,
                StockInicial = stockInicial,
                StockMinimo  = stockMinimo,
                StockMaximo  = stockMaximo,
                Descripcion  = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion,
                Proveedor    = string.IsNullOrWhiteSpace(FormProveedor)   ? null : FormProveedor,
                UsuarioId    = _session.UsuarioActual?.ID_Usuario ?? 0
            };

            var id = await _inventarioService.CrearProductoAsync(dto);
            if (id != null)
            {
                MensajeExito         = $"Producto '{dto.Nombre}' creado correctamente.";
                MostrarNuevoProducto = false;
                await CargarAsync();
            }
            else
            {
                MensajeErrorProducto = "No se pudo crear el producto. Verifique que el código no esté duplicado.";
            }
        }
        catch { MensajeErrorProducto = "Error al guardar el producto."; }
        finally { EstaCargando = false; }
    }

    [RelayCommand]
    private void CancelarNuevoProducto()
    {
        MostrarNuevoProducto = false;
        MensajeErrorProducto = null;
    }
}
