using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Portfolio.Web.Content;

// Formatação mínima para os textos do JSON:
//   *texto*   → <em> (itálico serifado nos títulos)
//   **texto** → <strong>
// O texto é codificado antes, então HTML digitado no JSON aparece como texto, não é executado.
public static partial class Rich
{
    public static MarkupString Render(string? text)
    {
        var html = WebUtility.HtmlEncode(text ?? "");
        html = Bold().Replace(html, "<strong>$1</strong>");
        html = Italic().Replace(html, "<em>$1</em>");
        return new MarkupString(html);
    }

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex Bold();

    [GeneratedRegex(@"\*(.+?)\*")]
    private static partial Regex Italic();
}
