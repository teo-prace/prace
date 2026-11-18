namespace Portfolio.Web;

// Dados de marca e contato, configurados em appsettings.json (seção "Site").
public sealed class SiteOptions
{
    public string Brand { get; set; } = "";
    // Nome completo do responsável (rodapé). Quando houver CNPJ, pode virar a razão social.
    public string Owner { get; set; } = "";
    public string Email { get; set; } = "";
    public string LinkedInUrl { get; set; } = "";
    public string GitHubUrl { get; set; } = "";
    // Só dígitos, com DDI e DDD. Ex.: 5551999999999
    public string WhatsApp { get; set; } = "";
    // Logo padrão: dot | horizontal | compact | bridge | syne | stacked (veja Shared/PraceLogo.razor)
    public string Logo { get; set; } = "compact";
    // Paleta de destaque: verde | marinho | latao | terracota | grafite (veja app.css)
    public string Palette { get; set; } = "verde";
}
