using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;

namespace GestionComercial.Presentation.ViewModels.Modules;

public partial class UsuariosViewModel : ObservableObject
{
    private readonly IUsuarioService _usuarioService;

    [ObservableProperty] private ObservableCollection<UsuarioDTO> _usuarios = new();
    [ObservableProperty] private UsuarioDTO? _usuarioSeleccionado;
    [ObservableProperty] private bool _estaCargando;
    [ObservableProperty] private string? _mensajeError;
    [ObservableProperty] private string? _mensajeExito;
    [ObservableProperty] private string _filtro = string.Empty;

    // Formulario
    [ObservableProperty] private bool _mostrarFormulario;
    [ObservableProperty] private bool _esEdicion;
    [ObservableProperty] private string _formNombreUsuario = string.Empty;
    [ObservableProperty] private string _formNombreCompleto = string.Empty;
    [ObservableProperty] private string _formEmail = string.Empty;
    [ObservableProperty] private string _formTelefono = string.Empty;
    [ObservableProperty] private int _formRolId = 2;
    [ObservableProperty] private string _formContrasena = string.Empty;

    private List<UsuarioDTO> _todosLosUsuarios = new();

    public UsuariosViewModel(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        EstaCargando = true;
        MensajeError = null;
        try
        {
            _todosLosUsuarios = (await _usuarioService.ObtenerActivosAsync()).ToList();
            AplicarFiltro();
        }
        catch
        {
            MensajeError = "Error al cargar usuarios.";
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
            ? _todosLosUsuarios
            : _todosLosUsuarios.Where(u =>
                u.NombreCompleto.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                u.NombreUsuario.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(Filtro, StringComparison.OrdinalIgnoreCase));

        Usuarios = new ObservableCollection<UsuarioDTO>(lista);
    }

    [RelayCommand]
    private void NuevoUsuario()
    {
        LimpiarFormulario();
        EsEdicion = false;
        MostrarFormulario = true;
    }

    [RelayCommand]
    private void EditarUsuario(UsuarioDTO? usuario)
    {
        if (usuario == null) return;
        UsuarioSeleccionado = usuario;
        FormNombreUsuario = usuario.NombreUsuario;
        FormNombreCompleto = usuario.NombreCompleto;
        FormEmail = usuario.Email;
        FormTelefono = usuario.Telefono;
        FormRolId = usuario.ID_Rol;
        FormContrasena = string.Empty;
        EsEdicion = true;
        MostrarFormulario = true;
    }

    [RelayCommand]
    private async Task GuardarUsuarioAsync()
    {
        MensajeError = null;
        MensajeExito = null;

        if (string.IsNullOrWhiteSpace(FormNombreUsuario) || string.IsNullOrWhiteSpace(FormNombreCompleto))
        {
            MensajeError = "Nombre de usuario y nombre completo son obligatorios.";
            return;
        }

        if (!EsEdicion && string.IsNullOrWhiteSpace(FormContrasena))
        {
            MensajeError = "La contraseña es obligatoria para nuevos usuarios.";
            return;
        }

        EstaCargando = true;
        try
        {
            var dto = new CrearActualizarUsuarioDTO
            {
                NombreUsuario = FormNombreUsuario,
                NombreCompleto = FormNombreCompleto,
                Email = FormEmail,
                Telefono = string.IsNullOrWhiteSpace(FormTelefono) ? null : FormTelefono,
                ID_Rol = FormRolId,
                Contrasena = string.IsNullOrWhiteSpace(FormContrasena) ? null : FormContrasena
            };

            bool ok;
            if (EsEdicion && UsuarioSeleccionado != null)
                ok = await _usuarioService.ActualizarAsync(UsuarioSeleccionado.ID_Usuario, dto);
            else
                ok = (await _usuarioService.CrearAsync(dto)) != null;

            if (ok)
            {
                MensajeExito = EsEdicion ? "Usuario actualizado correctamente." : "Usuario creado correctamente.";
                MostrarFormulario = false;
                await CargarAsync();
            }
            else
            {
                MensajeError = EsEdicion ? "No se pudo actualizar el usuario." : "El nombre de usuario ya existe.";
            }
        }
        catch
        {
            MensajeError = "Error al guardar el usuario.";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task InactivarUsuarioAsync(UsuarioDTO? usuario)
    {
        if (usuario == null) return;
        EstaCargando = true;
        MensajeError = null;
        try
        {
            var ok = await _usuarioService.InactivarAsync(usuario.ID_Usuario);
            if (ok)
            {
                MensajeExito = $"Usuario '{usuario.NombreUsuario}' inactivado.";
                await CargarAsync();
            }
            else
            {
                MensajeError = "No se pudo inactivar el usuario.";
            }
        }
        catch { MensajeError = "Error al inactivar usuario."; }
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
        FormNombreUsuario = FormNombreCompleto = FormEmail =
        FormTelefono = FormContrasena = string.Empty;
        FormRolId = 2;
        MensajeError = null;
        MensajeExito = null;
    }
}
