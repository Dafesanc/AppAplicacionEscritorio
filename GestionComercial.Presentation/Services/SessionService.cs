using GestionComercial.Application.DTOs;

namespace GestionComercial.Presentation.Services;

public class SessionService
{
    public LoginResponseDTO? UsuarioActual { get; private set; }
    public bool EstaAutenticado => UsuarioActual != null;

    public void IniciarSesion(LoginResponseDTO usuario) => UsuarioActual = usuario;
    public void CerrarSesion() => UsuarioActual = null;
}
