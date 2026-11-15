using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Portfolio.Web;
using Portfolio.Web.Components;
using Portfolio.Web.Content;
using Portfolio.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
builder.Services.Configure<FeatureOptions>(builder.Configuration.GetSection("Features"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

builder.Services.AddSingleton<ContentService>();
builder.Services.AddSingleton<ArticleService>();

// E-mail real quando configurado (Azure); senão, só log (máquina local).
var email = builder.Configuration.GetSection("Email").Get<EmailOptions>() ?? new();
if (email.IsConfigured)
    builder.Services.AddHttpClient<IContactSender, ResendContactSender>();
else
    builder.Services.AddSingleton<IContactSender, LoggingContactSender>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

// "/" redireciona para /pt ou /en conforme o idioma do navegador.
app.MapGet("/", (HttpContext ctx) =>
{
    var preferred = ctx.Request.GetTypedHeaders().AcceptLanguage
        .OrderByDescending(l => l.Quality ?? 1)
        .Select(l => l.Value.Value ?? "")
        .FirstOrDefault() ?? "";
    var lang = preferred.Length == 0 || preferred.StartsWith("pt", StringComparison.OrdinalIgnoreCase) ? "pt" : "en";
    return Results.Redirect($"/{lang}");
});

// Usado pelo Azure para saber se o contêiner está saudável.
app.MapGet("/healthz", () => Results.Text("ok"));

app.MapGet("/robots.txt", (HttpContext ctx) =>
    Results.Text($"User-agent: *\nAllow: /\nSitemap: {ctx.Request.Scheme}://{ctx.Request.Host}/sitemap.xml\n"));

app.MapGet("/sitemap.xml", (HttpContext ctx, ArticleService articles, IOptions<FeatureOptions> features) =>
{
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    var paths = new List<string>();
    foreach (var lang in ContentService.Languages)
    {
        paths.AddRange([$"/{lang}", $"/{lang}/services", $"/{lang}/cases", $"/{lang}/about", $"/{lang}/contact"]);
        if (features.Value.Articles && articles.List(lang).Count > 0)
        {
            paths.Add($"/{lang}/articles");
            paths.AddRange(articles.List(lang).Select(a => $"/{lang}/articles/{a.Slug}"));
        }
    }

    var xml = new StringBuilder("""<?xml version="1.0" encoding="UTF-8"?><urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");
    foreach (var path in paths)
        xml.Append($"<url><loc>{baseUrl}{path}</loc></url>");
    xml.Append("</urlset>");
    return Results.Content(xml.ToString(), "application/xml");
});

app.MapRazorComponents<App>();

app.Run();
