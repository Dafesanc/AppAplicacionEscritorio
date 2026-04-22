using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Presentation.Services;
using GestionComercial.Presentation.ViewModels.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace GestionComercial.Presentation.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    private readonly SessionService _session;

    [ObservableProperty] private ObservableObject? _currentViewModel;
    [ObservableProperty] private string _usuarioNombre = string.Empty;
    [ObservableProperty] private string _usuarioRol = string.Empty;
    [ObservableProperty] private string _tituloModulo = "Dashboard";
    [ObservableProperty] private bool _dashboardSeleccionado = true;
    [ObservableProperty] private bool _ventasSeleccionado;
    [ObservableProperty] private bool _pesajesSeleccionado;
    [ObservableProperty] private bool _clientesSeleccionado;
    [ObservableProperty] private bool _inventarioSeleccionado;
    [ObservableProperty] private bool _facturasSeleccionado;
    [ObservableProperty] private bool _usuariosSeleccionado;
    [ObservableProperty] private bool _vehiculosSeleccionado;

    public event Action? SolicitudCerrarSesion;

    public ShellViewModel(IServiceProvider services, SessionService session)
    {
        _services = services;
        _session = session;
    }

    public void Inicializar(LoginResponseDTO usuario)
    {
        _session.IniciarSesion(usuario);
        UsuarioNombre = usuario.NombreCompleto;
        UsuarioRol = usuario.RolNombre;
        IrDashboard();
    }

    [RelayCommand]
    private void IrDashboard()
    {
        LimpiarSeleccion();
        DashboardSeleccionado = true;
        TituloModulo = "Dashboard";
        CurrentViewModel = _services.GetRequiredService<DashboardViewModel>();
    }

    [RelayCommand]
    private void IrVentas()
    {
        LimpiarSeleccion();
        VentasSeleccionado = true;
        TituloModulo = "Ventas";
        CurrentViewModel = _services.GetRequiredService<VentasViewModel>();
    }

    [RelayCommand]
    private void IrPesajes()
    {
        LimpiarSeleccion();
        PesajesSeleccionado = true;
        TituloModulo = "Pesajes / Báscula";
        CurrentViewModel = _services.GetRequiredService<PesajesViewModel>();
    }

    [RelayCommand]
    private void IrClientes()
    {
        LimpiarSeleccion();
        ClientesSeleccionado = true;
        TituloModulo = "Clientes y Vehículos";
        CurrentViewModel = _services.GetRequiredService<ClientesViewModel>();
    }

    [RelayCommand]
    private void IrInventario()
    {
        LimpiarSeleccion();
        InventarioSeleccionado = true;
        TituloModulo = "Inventario";
        CurrentViewModel = _services.GetRequiredService<InventarioViewModel>();
    }

    [RelayCommand]
    private void IrFacturas()
    {
        LimpiarSeleccion();
        FacturasSeleccionado = true;
        TituloModulo = "Facturación";
        CurrentViewModel = _services.GetRequiredService<FacturasViewModel>();
    }

    [RelayCommand]
    private void IrUsuarios()
    {
        LimpiarSeleccion();
        UsuariosSeleccionado = true;
        TituloModulo = "Usuarios y Roles";
        CurrentViewModel = _services.GetRequiredService<UsuariosViewModel>();
    }

    [RelayCommand]
    private void IrVehiculos()
    {
        LimpiarSeleccion();
        VehiculosSeleccionado = true;
        TituloModulo = "Vehículos";
        CurrentViewModel = _services.GetRequiredService<VehiculosViewModel>();
    }

    [RelayCommand]
    private void CerrarSesion()
    {
        _session.CerrarSesion();
        SolicitudCerrarSesion?.Invoke();
    }

    private void LimpiarSeleccion()
    {
        DashboardSeleccionado = false;
        VentasSeleccionado = false;
        PesajesSeleccionado = false;
        ClientesSeleccionado = false;
        InventarioSeleccionado = false;
        FacturasSeleccionado  = false;
        UsuariosSeleccionado  = false;
        VehiculosSeleccionado = false;
    }
}
