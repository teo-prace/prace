using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Portfolio.Web.Services;

public sealed record ContactMessage(string Name, string Email, string Message, string Language);

public interface IContactSender
{
    Task SendAsync(ContactMessage message, CancellationToken ct = default);
}

// Configuração em appsettings.json, seção "Email".
// A chave da API NUNCA vai para o repositório: no Azure ela fica como secret.
public sealed class EmailOptions
{
    public string ResendApiKey { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ResendApiKey) && !string.IsNullOrWhiteSpace(From) && !string.IsNullOrWhiteSpace(To);
}

// Usado quando o e-mail não está configurado (ex.: na sua máquina): só registra no log.
public sealed class LoggingContactSender(ILogger<LoggingContactSender> logger) : IContactSender
{
    public Task SendAsync(ContactMessage message, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Novo contato ({Language}) de {Name} <{Email}>: {Message}",
            message.Language, message.Name, message.Email, message.Message);
        return Task.CompletedTask;
    }
}

// Envio real via Resend (https://resend.com). Plano gratuito atende um portfólio.
public sealed class ResendContactSender(HttpClient http, IOptions<EmailOptions> options) : IContactSender
{
    public async Task SendAsync(ContactMessage message, CancellationToken ct = default)
    {
        var o = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = JsonContent.Create(new
            {
                from = o.From,
                to = new[] { o.To },
                reply_to = message.Email,
                subject = $"[Site] Novo contato de {message.Name}",
                text = $"Nome: {message.Name}\nE-mail: {message.Email}\nIdioma: {message.Language}\n\n{message.Message}"
            })
        };
        request.Headers.Authorization = new("Bearer", o.ResendApiKey);

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
