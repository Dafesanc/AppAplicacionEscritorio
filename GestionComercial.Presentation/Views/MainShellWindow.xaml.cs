using System.Windows;
using GestionComercial.Application.DTOs;
using GestionComercial.Presentation.ViewModels;
using GestionComercial.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GestionComercial.Presentation.Views;

public partial class MainShellWindow : Window
{
    private readonly ShellViewModel    _vm;
    private readonly IServiceProvider  _services;
    private readonly ChatBotViewModel  _chatBotVm;

    public MainShellWindow(ShellViewModel vm, IServiceProvider services)
    {
        InitializeComponent();
        _vm       = vm;
        _services = services;
        DataContext = vm;

        // Inicializar asistente virtual
        _chatBotVm = services.GetRequiredService<ChatBotViewModel>();
        ChatBotControl.DataContext = _chatBotVm;
        ChatBotControl.BindViewModel(_chatBotVm);

        // Navegación solicitada por el chatbot → delegar al ShellViewModel
        _chatBotVm.NavigacionSolicitada += modulo =>
        {
            _chatBotVm.ModuloActivo = _vm.TituloModulo;
            _vm.NavegateToModule(modulo);
        };

        vm.SolicitudCerrarSesion += OnCerrarSesion;
    }

    public void Inicializar(LoginResponseDTO usuario)
    {
        _vm.Inicializar(usuario);
    }

    private void OnCerrarSesion()
    {
        var loginWindow = _services.GetRequiredService<LoginWindow>();
        // Reiniciar LoginWindow con un ViewModel limpio
        var loginVm = _services.GetRequiredService<LoginViewModel>();
        loginWindow.DataContext = loginVm;
        loginWindow.Show();
        Close();
    }
}
