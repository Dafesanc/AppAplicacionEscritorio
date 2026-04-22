using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;

namespace GestionComercial.Presentation.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUsuarioService _usuarioService;

    [ObservableProperty] private string _nombreUsuario = string.Empty;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private bool _estaCargando;

    // La contraseña se pasa desde el code-behind por seguridad (PasswordBox no bindea)
    public string Contrasena { get; set; } = string.Empty;

    public event Action<LoginResponseDTO>? LoginExitoso;

    public LoginViewModel(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [RelayCommand]
    public async Task IniciarSesionAsync()
    {
        MensajeError = null;

        if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Contrasena))
        {
            MensajeError = "Ingrese usuario y contraseña.";
            return;
        }

        EstaCargando = true;
        try
        {
            var resultado = await _usuarioService.AutenticarAsync(new LoginDTO
            {
                NombreUsuario = NombreUsuario,
                Contrasena = Contrasena
            });

            if (resultado == null)
            {
                MensajeError = "Usuario o contraseña incorrectos.";
                return;
            }

            LoginExitoso?.Invoke(resultado);
        }
        catch
        {
            MensajeError = "No se pudo conectar a la base de datos. Verifique la conexión.";
        }
        finally
        {
            EstaCargando = false;
        }
    }
}
