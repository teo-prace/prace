using System.Text.Json;
using Portfolio.Web.Content;

namespace Portfolio.Web.Tests;

// Garante que todos os idiomas têm os mesmos textos: se você adicionar algo em pt
// e esquecer do en (ou deixar um texto vazio), este teste falha.
public class ContentParityTests
{
    private static readonly JsonDocumentOptions Options = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    [Fact]
    public void All_languages_have_the_same_structure_and_no_empty_texts()
    {
        var reference = Load(ContentService.DefaultLanguage);
        var problems = new List<string>();

        foreach (var lang in ContentService.Languages)
        {
            var doc = Load(lang);
            CheckNoEmpty(doc.RootElement, lang, "", problems);
            if (lang != ContentService.DefaultLanguage)
                Compare(reference.RootElement, doc.RootElement, lang, "", problems);
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void Articles_have_a_translation_in_every_language()
    {
        var root = Paths.ContentRoot();
        var slugsByLang = ContentService.Languages.ToDictionary(
            lang => lang,
            lang => Directory.Exists(Path.Combine(root, lang, "articles"))
                ? Directory.GetFiles(Path.Combine(root, lang, "articles"), "*.md").Select(Path.GetFileNameWithoutExtension).ToHashSet()
                : []);

        var all = slugsByLang.Values.SelectMany(s => s).ToHashSet();
        var missing = slugsByLang
            .SelectMany(kv => all.Except(kv.Value).Select(slug => $"{kv.Key}/articles/{slug}.md"))
            .ToList();

        Assert.True(missing.Count == 0, "Artigos sem tradução: " + string.Join(", ", missing));
    }

    private static JsonDocument Load(string lang) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(Paths.ContentRoot(), lang, "site.json")), Options);

    private static void Compare(JsonElement expected, JsonElement actual, string lang, string path, List<string> problems)
    {
        if (expected.ValueKind != actual.ValueKind)
        {
            problems.Add($"[{lang}] {path}: tipo diferente ({expected.ValueKind} x {actual.ValueKind})");
            return;
        }

        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                var expectedKeys = expected.EnumerateObject().Select(p => p.Name).ToHashSet();
                var actualKeys = actual.EnumerateObject().Select(p => p.Name).ToHashSet();
                foreach (var key in expectedKeys.Except(actualKeys)) problems.Add($"[{lang}] {path}.{key}: faltando");
                foreach (var key in actualKeys.Except(expectedKeys)) problems.Add($"[{lang}] {path}.{key}: sobrando");
                foreach (var key in expectedKeys.Intersect(actualKeys))
                    Compare(expected.GetProperty(key), actual.GetProperty(key), lang, $"{path}.{key}", problems);
                break;

            case JsonValueKind.Array:
                var e = expected.EnumerateArray().ToList();
                var a = actual.EnumerateArray().ToList();
                if (e.Count != a.Count)
                    problems.Add($"[{lang}] {path}: {a.Count} itens, esperado {e.Count}");
                for (var i = 0; i < Math.Min(e.Count, a.Count); i++)
                    Compare(e[i], a[i], lang, $"{path}[{i}]", problems);
                break;
        }
    }

    private static void CheckNoEmpty(JsonElement element, string lang, string path, List<string> problems)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String when string.IsNullOrWhiteSpace(element.GetString()):
                problems.Add($"[{lang}] {path}: texto vazio");
                break;
            case JsonValueKind.Object:
                foreach (var p in element.EnumerateObject()) CheckNoEmpty(p.Value, lang, $"{path}.{p.Name}", problems);
                break;
            case JsonValueKind.Array:
                var i = 0;
                foreach (var item in element.EnumerateArray()) CheckNoEmpty(item, lang, $"{path}[{i++}]", problems);
                break;
        }
    }
}

internal static class Paths
{
    public static string ContentRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Portfolio.slnx")))
            dir = dir.Parent;
        if (dir is null) throw new InvalidOperationException("Portfolio.slnx não encontrado.");
        return Path.Combine(dir.FullName, "src", "Portfolio.Web", "Content");
    }
}
