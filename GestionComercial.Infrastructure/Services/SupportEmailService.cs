using GestionComercial.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace GestionComercial.Infrastructure.Services;

public class SupportEmailService : ISupportEmailService
{
    private readonly string _smtpHost;
    private readonly int    _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _supportEmail;
    private readonly string _senderName;

    public SupportEmailService(IConfiguration config)
    {
        _smtpHost     = config["Email:SmtpHost"]     ?? "smtp.gmail.com";
        _smtpPort     = int.TryParse(config["Email:SmtpPort"], out var p) ? p : 587;
        _smtpUser     = config["Email:SmtpUser"]     ?? string.Empty;
        _smtpPassword = config["Email:SmtpPassword"] ?? string.Empty;
        _supportEmail = config["Email:SupportEmail"] ?? "dafesanc12@gmail.com";
        _senderName   = config["Email:SenderName"]   ?? "GestiónComercial";
    }

    public async Task<bool> EnviarReporteAsync(
        string mensaje, string usuarioNombre, string usuarioRol, string moduloActivo)
    {
        if (string.IsNullOrWhiteSpace(_smtpUser) || string.IsNullOrWhiteSpace(_smtpPassword))
            return false;

        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_senderName, _smtpUser));
            email.To.Add(new MailboxAddress("Soporte Técnico", _supportEmail));
            email.Subject = $"[Soporte] {usuarioNombre} — {DateTime.Now:dd/MM/yyyy HH:mm}";
            email.Body = new TextPart("plain")
            {
                Text = $"""
                    REPORTE DE SOPORTE — GestiónComercial
                    =======================================
                    Usuario:        {usuarioNombre}
                    Rol:            {usuarioRol}
                    Módulo activo:  {(string.IsNullOrEmpty(moduloActivo) ? "No especificado" : moduloActivo)}
                    Fecha/Hora:     {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                    Versión app:    1.0.0
                    =======================================

                    DESCRIPCIÓN DEL PROBLEMA:
                    {mensaje}

                    =======================================
                    Enviado automáticamente por el Asistente Virtual de GestiónComercial.
                    """
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpUser, _smtpPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
