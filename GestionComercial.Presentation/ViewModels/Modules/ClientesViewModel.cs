using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class ClientesViewModel : ObservableObject
{
    private readonly IClienteService _clienteService;

    [ObservableProperty] private ObservableCollection<ClienteDTO> _clientes = new();
    [ObservableProperty] private ClienteDTO? _clienteSeleccionado;
    [ObservableProperty] private string _filtro = string.Empty;
    [ObservableProperty] private bool _estaCargando;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private string? _mensajeExito;

    // Formulario
    [ObservableProperty] private bool _mostrarFormulario;
    [ObservableProperty] private bool _esEdicion;
    [ObservableProperty] private string _formNombre = string.Empty;
    [ObservableProperty] private string _formTipoIdentificacion = "RUC";
    [ObservableProperty] private string _formNumeroIdentificacion = string.Empty;
    [ObservableProperty] private string _formCategoria = "Transportista";
    [ObservableProperty] private string _formEmail = string.Empty;
    [ObservableProperty] private string _formTelefono = string.Empty;
    [ObservableProperty] private string _formDireccion = string.Empty;
    [ObservableProperty] private decimal _formDescuento;
    [ObservableProperty] private int _formPlazoCredito;
    [ObservableProperty] private decimal _formLimiteCredito;
    [ObservableProperty] private string _formObservaciones = string.Empty;

    public string[] TiposIdentificacion { get; } = { "RUC", "CÉDULA", "PASAPORTE" };
    public string[] Categorias { get; } = { "Transportista", "Mayorista", "Minorista" };

    private List<ClienteDTO> _todosLosClientes = new();

    public ClientesViewModel(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            _todosLosClientes = (await _clienteService.ObtenerActivosAsync()).ToList();
            AplicarFiltro();
        }
        catch
        {
            MensajeError = "Error al cargar clientes.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    partial void OnFiltroChanged(string value) => AplicarFiltro();

    private void AplicarFiltro()
    {
        var lista = string.IsNullOrWhiteSpace(Filtro)
            ? _todosLosClientes
            : _todosLosClientes.Where(c =>
                c.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                c.NumeroIdentificacion.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                c.CodigoCliente.Contains(Filtro, StringComparison.OrdinalIgnoreCase));

        Clientes = new ObservableCollection<ClienteDTO>(lista);
    }

    [RelayCommand]
    private void NuevoCliente()
    {
        LimpiarFormulario();
        EsEdicion = false;
        MostrarFormulario = true;
    }

    [RelayCommand]
    private void EditarCliente(ClienteDTO? cliente)
    {
        if (cliente == null) return;
        ClienteSeleccionado = cliente;
        CargarFormulario(cliente);
        EsEdicion = true;
        MostrarFormulario = true;
    }

    [RelayCommand]
    private async Task GuardarClienteAsync()
    {
        MensajeError = null;
        MensajeExito = null;

        if (string.IsNullOrWhiteSpace(FormNombre) || string.IsNullOrWhiteSpace(FormNumeroIdentificacion))
        {
            MensajeError = "Nombre e identificación son obligatorios.";
            return;
        }

        EstaCargando = true;
        try
        {
            var dto = new CrearActualizarClienteDTO
            {
                Nombre = FormNombre,
                TipoIdentificacion = FormTipoIdentificacion,
                NumeroIdentificacion = FormNumeroIdentificacion,
                Categoria = FormCategoria,
                Email = string.IsNullOrWhiteSpace(FormEmail) ? null : FormEmail,
                Telefono = string.IsNullOrWhiteSpace(FormTelefono) ? null : FormTelefono,
                Direccion = string.IsNullOrWhiteSpace(FormDireccion) ? null : FormDireccion,
                DescuentoPorDefecto = FormDescuento,
                PlazoCredito = FormPlazoCredito,
                LimiteCredito = FormLimiteCredito,
                Observaciones = string.IsNullOrWhiteSpace(FormObservaciones) ? null : FormObservaciones
            };

            bool ok;
            if (EsEdicion && ClienteSeleccionado != null)
                ok = await _clienteService.ActualizarAsync(ClienteSeleccionado.IdCliente, dto);
            else
                ok = (await _clienteService.CrearAsync(dto)) != null;

            if (ok)
            {
                MensajeExito = EsEdicion ? "Cliente actualizado correctamente." : "Cliente creado correctamente.";
                MostrarFormulario = false;
                await CargarAsync();
            }
            else
            {
                MensajeError = EsEdicion ? "No se pudo actualizar el cliente." : "Identificación ya registrada.";
            }
        }
        catch
        {
            MensajeError = "Error al guardar el cliente.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task BloquearClienteAsync(ClienteDTO? cliente)
    {
        if (cliente == null) return;
        EstaCargando = true;
        MensajeError = null;
        try
        {
            var ok = await _clienteService.BloquearAsync(cliente.IdCliente);
            if (ok)
            {
                MensajeExito = "Cliente bloqueado.";
                await CargarAsync();
            }
        }
        catch { MensajeError = "Error al bloquear el cliente."; }
        finally { EstaCargando = false; }
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormulario = false;
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        FormNombre = FormNumeroIdentificacion = FormEmail = FormTelefono =
        FormDireccion = FormObservaciones = string.Empty;
        FormTipoIdentificacion = "RUC";
        FormCategoria = "Transportista";
        FormDescuento = 0;
        FormPlazoCredito = 0;
        FormLimiteCredito = 0;
        MensajeError = null;
        MensajeExito = null;
    }

    private void CargarFormulario(ClienteDTO c)
    {
        FormNombre = c.Nombre;
        FormTipoIdentificacion = c.TipoIdentificacion;
        FormNumeroIdentificacion = c.NumeroIdentificacion;
        FormCategoria = c.Categoria;
        FormEmail = c.Email ?? string.Empty;
        FormTelefono = c.Telefono ?? string.Empty;
        FormDireccion = c.Direccion ?? string.Empty;
        FormDescuento = c.DescuentoPorDefecto;
        FormPlazoCredito = c.PlazoCredito;
        FormLimiteCredito = c.LimiteCredito;
        FormObservaciones = c.Observaciones ?? string.Empty;
    }
}
