using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionComercial.Application.Services;
using GestionComercial.Presentation.Services;

namespace GestionComercial.Presentation.ViewModels;

public class ChatMessageItem
{
    public bool   IsUser    { get; init; }
    public string Content   { get; init; } = string.Empty;
    public bool   IsThinking { get; init; }
}

public partial class ChatBotViewModel : ObservableObject
{
    private readonly IChatbotService     _chatbotService;
    private readonly ISupportEmailService _emailService;
    private readonly SessionService      _sessionService;

    [ObservableProperty] private bool   _chatVisible;
    [ObservableProperty] private string _inputText  = string.Empty;
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private ObservableCollection<ChatMessageItem> _messages = new();

    private bool   _awaitingSupport;
    public  string ModuloActivo { get; set; } = string.Empty;

    public event Action<string>? NavigacionSolicitada;

    public ChatBotViewModel(
        IChatbotService      chatbotService,
        ISupportEmailService emailService,
        SessionService       sessionService)
    {
        _chatbotService = chatbotService;
        _emailService   = emailService;
        _sessionService = sessionService;

        AddBotMessage("¡Hola! 👋 Soy tu asistente virtual. Puedo ayudarte a navegar por el sistema, ver tu perfil o reportar una falla. ¿En qué te ayudo?");
    }

    // ── Comandos principales ─────────────────────────────────────────────────

    [RelayCommand]
    private void ToggleChat() => ChatVisible = !ChatVisible;

    [RelayCommand]
    private void VerMisDatos()
    {
        ChatVisible = true;
        MostrarDatosUsuario();
    }

    [RelayCommand]
    private void IniciarSoporte()
    {
        ChatVisible     = true;
        _awaitingSupport = true;
        AddBotMessage("🐛 Cuéntame el problema o error que encontraste y lo reportaré al equipo de soporte:");
    }

    // mensaje: null → usa InputText; string → viene de botón rápido
    [RelayCommand]
    private async Task EnviarMensaje(string? mensaje)
    {
        var texto = mensaje ?? InputText.Trim();
        if (string.IsNullOrWhiteSpace(texto) || IsBusy) return;

        if (mensaje == null) InputText = string.Empty;
        AddUserMessage(texto);

        if (_awaitingSupport)
        {
            await ProcesarMensajeSoporte(texto);
            return;
        }

        await ProcesarConIA(texto);
    }

    // ── Lógica interna ───────────────────────────────────────────────────────

    private async Task ProcesarConIA(string texto)
    {
        IsBusy = true;
        var thinking = new ChatMessageItem { IsThinking = true, Content = "⏳ Procesando..." };
        Messages.Add(thinking);

        try
        {
            var response = await _chatbotService.ProcessAsync(texto);
            Messages.Remove(thinking);
            AddBotMessage(response.Message);

            switch (response.Intent)
            {
                case "NAVIGATE" when !string.IsNullOrWhiteSpace(response.Module):
                    await Task.Delay(500);
                    ChatVisible = false;
                    NavigacionSolicitada?.Invoke(response.Module);
                    break;
                case "SUPPORT":
                    _awaitingSupport = true;
                    break;
                case "USER_DATA":
                    MostrarDatosUsuario();
                    break;
            }
        }
        catch
        {
            Messages.Remove(thinking);
            AddBotMessage("Ocurrió un error. Intenta de nuevo.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ProcesarMensajeSoporte(string mensaje)
    {
        _awaitingSupport = false;

        var confirm = MessageBox.Show(
            $"¿Deseas enviar este reporte al equipo de soporte?\n\n\"{mensaje}\"",
            "Confirmar reporte",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            AddBotMessage("Entendido, cancelé el reporte. ¿Necesitas algo más?");
            return;
        }

        IsBusy = true;
        AddBotMessage("Enviando reporte... ⏳");

        try
        {
            var usuario = _sessionService.UsuarioActual;
            var ok = await _emailService.EnviarReporteAsync(
                mensaje,
                usuario?.NombreCompleto ?? "Desconocido",
                usuario?.RolNombre     ?? "Sin rol",
                ModuloActivo);

            Messages.RemoveAt(Messages.Count - 1);
            AddBotMessage(ok
                ? "✅ Reporte enviado. El equipo de soporte te contactará pronto."
                : "⚠️ No se pudo enviar el correo. Verifica la configuración SMTP en appsettings.json.");
        }
        catch
        {
            Messages.RemoveAt(Messages.Count - 1);
            AddBotMessage("❌ Error al enviar el reporte. Verifica la configuración de correo.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void MostrarDatosUsuario()
    {
        var u = _sessionService.UsuarioActual;
        if (u == null) { AddBotMessage("No hay sesión activa."); return; }
        AddBotMessage($"👤 Tu información:\n• Nombre: {u.NombreCompleto}\n• Usuario: {u.NombreUsuario}\n• Rol: {u.RolNombre}");
    }

    private void AddBotMessage(string content)  => Messages.Add(new ChatMessageItem { IsUser = false, Content = content });
    private void AddUserMessage(string content) => Messages.Add(new ChatMessageItem { IsUser = true,  Content = content });
}
