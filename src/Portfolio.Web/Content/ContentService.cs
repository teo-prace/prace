using System.Text.Json;

namespace Portfolio.Web.Content;

// Carrega os textos de cada idioma (Content/{lang}/site.json) uma vez, na inicialização.
// Para adicionar um idioma: inclua o código em Languages e crie Content/{código}/site.json.
public sealed class ContentService
{
    public static readonly string[] Languages = ["pt", "en"];
    public const string DefaultLanguage = "pt";

    private readonly Dictionary<string, SiteContent> _content;

    public ContentService(IWebHostEnvironment env)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        _content = Languages.ToDictionary(
            lang => lang,
            lang =>
            {
                var path = Path.Combine(env.ContentRootPath, "Content", lang, "site.json");
                return JsonSerializer.Deserialize<SiteContent>(File.ReadAllText(path), options)
                    ?? throw new InvalidOperationException($"Conteúdo inválido: {path}");
            });
    }

    public static bool IsSupported(string? lang) => lang is not null && Languages.Contains(lang);

    public SiteContent Get(string? lang) => _content[IsSupported(lang) ? lang! : DefaultLanguage];

    // "/en/cases" -> "en"; qualquer outra coisa -> idioma padrão.
    public static string LanguageFromPath(string? path)
    {
        var first = (path ?? "").Trim('/').Split('/')[0];
        return IsSupported(first) ? first : DefaultLanguage;
    }

    // "/pt/cases" -> "/en/cases"
    public static string SwitchLanguagePath(string? path, string targetLang)
    {
        var segments = (path ?? "").Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (segments.Count > 0 && IsSupported(segments[0]))
            segments[0] = targetLang;
        else
            segments.Insert(0, targetLang);
        return "/" + string.Join('/', segments);
    }

    public static string HtmlLang(string lang) => lang == "pt" ? "pt-BR" : "en";
}
