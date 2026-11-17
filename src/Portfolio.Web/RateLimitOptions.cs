namespace Portfolio.Web;

// Limites por IP (appsettings.json, seção "RateLimit"). Veja Program.cs.
public sealed class RateLimitOptions
{
    // Páginas e arquivos: uma visita normal faz poucas requisições por página.
    public int RequestsPerMinute { get; set; } = 120;

    // Envios do formulário de contato por IP dentro da janela.
    public int ContactPerWindow { get; set; } = 5;
    public int ContactWindowMinutes { get; set; } = 10;
}
