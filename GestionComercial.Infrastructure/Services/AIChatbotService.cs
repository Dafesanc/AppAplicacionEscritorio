using System.Net.Http.Json;
using System.Text.Json;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;
using Microsoft.Extensions.Configuration;

namespace GestionComercial.Infrastructure.Services;

/// <summary>
/// Servicio de IA multi-proveedor. Configura "Provider" en appsettings.json:
/// "gemini" → Google Gemini Flash (free tier: 1500 req/día)
/// "groq"   → Groq + Llama (free tier: 14400 req/día)
/// "ollama" → Ollama local (gratuito, sin límites, requiere instalación local)
/// </summary>
public class AIChatbotService : IChatbotService
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly string _provider;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _ollamaUrl;

    private const string SystemPrompt =
        """
        Eres el asistente virtual de GestiónComercial, aplicación para gestión de materiales de construcción.
        Responde ÚNICAMENTE con un JSON válido, sin texto adicional ni markdown.

        Formato exacto de respuesta:
        {"intent":"<INTENT>","module":"<MODULE_O_NULL>","message":"<mensaje_amable>"}

        Valores de intent:
        - NAVIGATE: el usuario quiere ir a una sección del sistema
        - USER_DATA: el usuario quiere ver su perfil o datos de sesión
        - SUPPORT: tiene un problema, error o duda técnica
        - GREETING: saludo o conversación general
        - UNKNOWN: intención no identificada

        Módulos válidos para "module" (solo si intent=NAVIGATE, caso contrario null):
        dashboard, ventas, clientes, inventario, pesajes, facturas, usuarios, vehiculos

        El campo "message" debe ser una respuesta corta y amable en español (máx. 80 palabras).
        Responde solo el JSON, sin texto adicional.
        """;

    public AIChatbotService(IConfiguration config)
    {
        _provider  = config["Chatbot:Provider"]   ?? "gemini";
        _apiKey    = config["Chatbot:ApiKey"]      ?? string.Empty;
        _model     = config["Chatbot:Model"]       ?? DefaultModel(_provider);
        _ollamaUrl = config["Chatbot:OllamaUrl"]  ?? "http://localhost:11434";
    }

    public async Task<ChatbotResponseDTO> ProcessAsync(string userMessage)
    {
        try
        {
            var rawText = _provider.ToLowerInvariant() switch
            {
                "gemini" => await CallGeminiAsync(userMessage),
                "groq"   => await CallGroqAsync(userMessage),
                "ollama" => await CallOllamaAsync(userMessage),
                _        => null
            };

            if (rawText == null)
                return Fallback($"Proveedor '{_provider}' no reconocido. Usa: gemini, groq u ollama.");

            var result = JsonSerializer.Deserialize<ChatbotResponseDTO>(rawText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? Fallback();
        }
        catch (HttpRequestException)
        {
            return Fallback("No pude conectarme al asistente. Verifica tu conexión y API Key.");
        }
        catch
        {
            return Fallback();
        }
    }

    // ── Google Gemini ────────────────────────────────────────────────────────

    private async Task<string?> CallGeminiAsync(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return /*lang=json*/"""{"intent":"UNKNOWN","module":null,"message":"Configura tu API Key de Gemini en appsettings.json (Chatbot:ApiKey)."}""";

        var url  = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        var body = new
        {
            system_instruction = new { parts = new[] { new { text = SystemPrompt } } },
            contents           = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } },
            generationConfig   = new { maxOutputTokens = 300, temperature = 0.1 }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };
        using var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
    }

    // ── Groq (compatible con OpenAI) ─────────────────────────────────────────

    private async Task<string?> CallGroqAsync(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return /*lang=json*/"""{"intent":"UNKNOWN","module":null,"message":"Configura tu API Key de Groq en appsettings.json (Chatbot:ApiKey)."}""";

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = JsonContent.Create(new
        {
            model       = _model,
            max_tokens  = 300,
            temperature = 0.1,
            messages    = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = userMessage  }
            }
        });

        using var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    // ── Ollama (local) ───────────────────────────────────────────────────────

    private async Task<string?> CallOllamaAsync(string userMessage)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_ollamaUrl}/api/chat");
        request.Content = JsonContent.Create(new
        {
            model    = _model,
            stream   = false,
            messages = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = userMessage  }
            }
        });

        using var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    // ── Utilidades ───────────────────────────────────────────────────────────

    private static string DefaultModel(string provider) => provider.ToLowerInvariant() switch
    {
        "gemini" => "gemini-2.0-flash",
        "groq"   => "llama-3.1-8b-instant",
        "ollama" => "llama3.2:3b",
        _        => string.Empty
    };

    private static ChatbotResponseDTO Fallback(string? msg = null) => new()
    {
        Intent  = "UNKNOWN",
        Message = msg ?? "No pude procesar tu consulta. Intenta de nuevo."
    };
}
