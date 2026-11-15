using System.Globalization;
using Markdig;

namespace Portfolio.Web.Content;

public sealed record Article(
    string Slug,
    string Language,
    string Title,
    string Summary,
    DateOnly Date,
    IReadOnlyList<string> Tags,
    bool Draft,
    string Html);

// Lê os artigos de Content/{lang}/articles/*.md.
// O nome do arquivo é o endereço (slug). Use o MESMO nome em pt e en para ligar as traduções.
//
// Formato do arquivo:
//   ---
//   title: Título do artigo
//   summary: Resumo de uma ou duas frases
//   date: 2026-11-20
//   tags: .NET, WebForms
//   draft: true          (rascunhos só aparecem em ambiente de desenvolvimento)
//   ---
//   Texto em Markdown...
public sealed class ArticleService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private readonly Dictionary<string, List<Article>> _byLanguage;

    public ArticleService(IWebHostEnvironment env)
    {
        var includeDrafts = env.IsDevelopment();
        _byLanguage = ContentService.Languages.ToDictionary(
            lang => lang,
            lang =>
            {
                var dir = Path.Combine(env.ContentRootPath, "Content", lang, "articles");
                if (!Directory.Exists(dir)) return new List<Article>();
                return Directory.EnumerateFiles(dir, "*.md")
                    .Select(file => Parse(file, lang))
                    .Where(a => includeDrafts || !a.Draft)
                    .OrderByDescending(a => a.Date)
                    .ToList();
            });
    }

    public IReadOnlyList<Article> List(string lang) =>
        _byLanguage.TryGetValue(lang, out var list) ? list : [];

    public Article? Find(string lang, string slug) =>
        List(lang).FirstOrDefault(a => string.Equals(a.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<(string Lang, string Slug)> All() =>
        _byLanguage.SelectMany(kv => kv.Value.Select(a => (kv.Key, a.Slug)));

    private static Article Parse(string file, string lang)
    {
        var text = File.ReadAllText(file).Replace("\r\n", "\n");
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var body = text;

        if (text.StartsWith("---\n"))
        {
            var end = text.IndexOf("\n---", 4, StringComparison.Ordinal);
            if (end > 0)
            {
                foreach (var line in text[4..end].Split('\n'))
                {
                    var idx = line.IndexOf(':');
                    if (idx > 0) meta[line[..idx].Trim()] = line[(idx + 1)..].Trim();
                }
                body = text[(end + 4)..].TrimStart('\n');
            }
        }

        var slug = Path.GetFileNameWithoutExtension(file);
        return new Article(
            Slug: slug,
            Language: lang,
            Title: meta.GetValueOrDefault("title", slug),
            Summary: meta.GetValueOrDefault("summary", ""),
            Date: DateOnly.TryParse(meta.GetValueOrDefault("date"), CultureInfo.InvariantCulture, out var d) ? d : DateOnly.MinValue,
            Tags: meta.GetValueOrDefault("tags", "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            Draft: bool.TryParse(meta.GetValueOrDefault("draft"), out var draft) && draft,
            Html: Markdown.ToHtml(body, Pipeline));
    }
}
