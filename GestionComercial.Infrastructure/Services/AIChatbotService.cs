using System.Net.Http.Json;
using System.Text.Json;
using GestionComercial.Application.DTOs;
using GestionComercial.Application.Services;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace GestionComercial.Infrastructure.Services;

/// <summary>
/// Servicio de IA multi-proveedor. Configura "Provider" en appsettings.json:
///   "gemini" → Google Gemini Flash  (free tier: 1 500 req/día)
///   "groq"   → Groq + Llama         (free tier: 14 400 req/día)
///   "ollama" → Ollama local          (gratuito, sin límites)
///   "openai" → OpenAI GPT            (de pago, SDK oficial)
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
        - NAVIGATE  : el usuario quiere ir a una sección del sistema
        - USER_DATA : el usuario quiere ver su perfil o datos de sesión
        - SUPPORT   : tiene un problema, error o duda técnica
        - GREETING  : saludo o conversación general
        - UNKNOWN   : intención no identificada

        Módulos válidos para "module" (solo si intent=NAVIGATE, en otro caso null):
        dashboard, ventas, clientes, inventario, pesajes, facturas, usuarios, vehiculos

        Ejemplos de mapeo:
        "quiero ver ventas"          → {"intent":"NAVIGATE","module":"ventas","message":"Te llevo a Ventas."}
        "mostrar inventario"         → {"intent":"NAVIGATE","module":"inventario","message":"Abriendo Inventario."}
        "ir a clientes"              → {"intent":"NAVIGATE","module":"clientes","message":"Yendo a Clientes."}
        "ver mis productos"          → {"intent":"NAVIGATE","module":"inventario","message":"Los productos están en Inventario."}
        "mis datos"                  → {"intent":"USER_DATA","module":null,"message":"Aquí está tu perfil."}
        "tengo un error"             → {"intent":"SUPPORT","module":null,"message":"Cuéntame el problema."}
        "hola"                       → {"intent":"GREETING","module":null,"message":"¡Hola! ¿En qué te ayudo?"}

        El campo "message" debe ser una respuesta corta y amable en español (máx. 60 palabras).
        Responde SOLO el JSON, sin texto adicional.
        """;

    public AIChatbotService(IConfiguration config)
    {
        _provider  = (config["Chatbot:Provider"]  ?? "gemini").ToLowerInvariant();
        _apiKey    =  config["Chatbot:ApiKey"]     ?? string.Empty;
        _model     =  config["Chatbot:Model"]      ?? DefaultModel(_provider);
        _ollamaUrl =  config["Chatbot:OllamaUrl"]  ?? "http://localhost:11434";
    }

    // ── Punto de entrada ─────────────────────────────────────────────────────

    public async Task<ChatbotResponseDTO> ProcessAsync(string userMessage)
    {
        try
        {
            return _provider switch
            {
                "gemini" => await CallGeminiAsync(userMessage),
                "groq"   => await CallGroqAsync(userMessage),
                "ollama" => await CallOllamaAsync(userMessage),
                "openai" => await CallOpenAIAsync(userMessage),
                _        => Error($"Proveedor '{_provider}' no reconocido. Usa: gemini, groq, ollama u openai.")
            };
        }
        catch (TaskCanceledException)
        {
            return Error("Tiempo de espera agotado (30 s). Verifica tu conexión a internet.");
        }
        catch (HttpRequestException ex)
        {
            return Error($"Sin conexión a internet: {Trim(ex.Message, 80)}");
        }
        catch (Exception ex)
        {
            return Error($"Error {ex.GetType().Name}: {Trim(ex.Message, 100)}");
        }
    }

    // ── Google Gemini ─────────────────────────────────────────────────────────

    private async Task<ChatbotResponseDTO> CallGeminiAsync(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return Error("Configura tu API Key de Gemini en appsettings.json → Chatbot:ApiKey.");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new
            {
                system_instruction = new { parts = new[] { new { text = SystemPrompt } } },
                contents           = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } },
                generationConfig   = new { maxOutputTokens = 300, temperature = 0.1 }
            })
        };

        using var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return Error(GeminiHttpError((int)response.StatusCode, body));

        return ParseGeminiResponse(body);
    }

    private static ChatbotResponseDTO ParseGeminiResponse(string body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Verificar si la respuesta fue bloqueada por filtros de seguridad
        if (root.TryGetProperty("promptFeedback", out var feedback) &&
            feedback.TryGetProperty("blockReason", out var reason))
            return Error($"Respuesta bloqueada por filtro de seguridad: {reason.GetString()}");

        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            return Error("Gemini no devolvió candidatos. Intenta reformular tu mensaje.");

        var textRaw = candidates[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        // Limpiar posibles bloques de código markdown ```json ... ```
        var json = textRaw.Trim();
        if (json.StartsWith("```")) json = json.Split('\n', 3).Length > 1
            ? string.Join("\n", json.Split('\n').Skip(1).TakeWhile(l => !l.StartsWith("```")))
            : json;

        var result = JsonSerializer.Deserialize<ChatbotResponseDTO>(json.Trim(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? Error("El asistente no devolvió una respuesta válida.");
    }

    private static string GeminiHttpError(int status, string body)
    {
        var hint = status switch
        {
            400 => "solicitud inválida (revisa el nombre del modelo en appsettings).",
            401 or 403 => "API Key sin permiso — actívala en aistudio.google.com.",
            404 => "modelo no encontrado — asegúrate de usar 'gemini-2.0-flash'.",
            429 => "límite de solicitudes alcanzado (free tier). Espera un momento.",
            500 or 503 => "error temporal del servidor de Google. Intenta de nuevo.",
            _ => $"error HTTP {status}."
        };

        try
        {
            using var doc = JsonDocument.Parse(body);
            var msg = doc.RootElement.GetProperty("error").GetProperty("message").GetString();
            return $"Gemini {status}: {msg}";
        }
        catch
        {
            return $"Gemini {status} — {hint}";
        }
    }

    // ── Groq (OpenAI-compatible) ──────────────────────────────────────────────

    private async Task<ChatbotResponseDTO> CallGroqAsync(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return Error("Configura tu API Key de Groq en appsettings.json → Chatbot:ApiKey.");

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
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return Error($"Groq {(int)response.StatusCode}: verifica tu API Key.");

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        var result = JsonSerializer.Deserialize<ChatbotResponseDTO>(text.Trim(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? Error("Groq no devolvió una respuesta válida.");
    }

    // ── Ollama (local) ────────────────────────────────────────────────────────

    private async Task<ChatbotResponseDTO> CallOllamaAsync(string userMessage)
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
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return Error($"Ollama {(int)response.StatusCode}: ¿está corriendo el servidor local?");

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        var result = JsonSerializer.Deserialize<ChatbotResponseDTO>(text.Trim(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? Error("Ollama no devolvió una respuesta válida.");
    }

    // ── OpenAI (SDK oficial) ──────────────────────────────────────────────────

    private async Task<ChatbotResponseDTO> CallOpenAIAsync(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return Error("Configura tu API Key de OpenAI en appsettings.json → Chatbot:ApiKey.");

        var client = new OpenAIClient(_apiKey);
        var chat   = client.GetChatClient(_model);

        var completion = await chat.CompleteChatAsync(
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(userMessage));

        var text = completion.Value.Content[0].Text?.Trim() ?? "{}";

        var result = JsonSerializer.Deserialize<ChatbotResponseDTO>(text,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? Error("OpenAI no devolvió una respuesta válida.");
    }

    // ── Utilidades ────────────────────────────────────────────────────────────

    private static string DefaultModel(string provider) => provider switch
    {
        "gemini" => "gemini-2.0-flash",
        "groq"   => "llama-3.1-8b-instant",
        "ollama" => "llama3.2:3b",
        "openai" => "gpt-4o-mini",
        _        => string.Empty
    };

    private static ChatbotResponseDTO Error(string mensaje) => new()
    {
        Intent  = "UNKNOWN",
        Message = mensaje
    };

    private static string Trim(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
