using System.Windows;
using System.Windows.Controls;
using GestionComercial.Application.DTOs;
using GestionComercial.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GestionComercial.Presentation.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;
    private readonly IServiceProvider _services;
    private bool _mostrarContrasena;

    public LoginWindow(LoginViewModel vm, IServiceProvider services)
    {
        InitializeComponent();
        _vm = vm;
        _services = services;
        DataContext = vm;

        vm.LoginExitoso += OnLoginExitoso;

        Loaded += (_, _) => TxtUsuario.Focus();
    }

    private void TxtContrasena_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.Contrasena = TxtContrasena.Password;
    }

    private void TxtContrasenaVisible_TextChanged(object sender, TextChangedEventArgs e)
    {
        _vm.Contrasena = TxtContrasenaVisible.Text;
    }

    private void BtnToggleContrasena_Click(object sender, RoutedEventArgs e)
    {
        _mostrarContrasena = !_mostrarContrasena;
        if (_mostrarContrasena)
        {
            TxtContrasenaVisible.Text       = TxtContrasena.Password;
            TxtContrasena.Visibility        = Visibility.Collapsed;
            TxtContrasenaVisible.Visibility = Visibility.Visible;
            IconoOjo.Text = "🔒";
            TxtContrasenaVisible.Focus();
            TxtContrasenaVisible.CaretIndex = TxtContrasenaVisible.Text.Length;
        }
        else
        {
            TxtContrasena.Password          = TxtContrasenaVisible.Text;
            TxtContrasenaVisible.Visibility = Visibility.Collapsed;
            TxtContrasena.Visibility        = Visibility.Visible;
            IconoOjo.Text = "👁";
            TxtContrasena.Focus();
        }
    }

    private void OnLoginExitoso(LoginResponseDTO usuario)
    {
        var mainWindow = _services.GetRequiredService<MainShellWindow>();
        mainWindow.Inicializar(usuario);
        mainWindow.Show();
        Close();
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    // Permite arrastrar la ventana sin barra de título
    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        DragMove();
    }
}
