using System.Text;
using System.Threading.RateLimiting;
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

// Limite de requisições por IP
// Com no máximo 1 instância no Azure
// appsettings.json (RateLimit).
var limits = builder.Configuration.GetSection("RateLimit").Get<RateLimitOptions>() ?? new();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Corpo próprio para o 429
    options.OnRejected = (context, ct) =>
        new ValueTask(context.HttpContext.Response.WriteAsync("Too many requests. Try again in a few minutes.", ct));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        // Sondas de saúde do Azure ficam fora do limite.
        if (ctx.Request.Path.StartsWithSegments("/healthz"))
            return RateLimitPartition.GetNoLimiter("healthz");

        // Atrás do ingress do Azure, o IP real vem do X-Forwarded-For (ASPNETCORE_FORWARDEDHEADERS_ENABLED).
        // Quando a Cloudflare estiver na frente (proxy), trocar para o cabeçalho CF-Connecting-IP.
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var isContactPost = HttpMethods.IsPost(ctx.Request.Method)
            && ctx.Request.Path.Value?.EndsWith("/contact", StringComparison.OrdinalIgnoreCase) == true;

        return isContactPost
            ? RateLimitPartition.GetFixedWindowLimiter("contact:" + ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limits.ContactPerWindow,
                Window = TimeSpan.FromMinutes(limits.ContactWindowMinutes),
            })
            : RateLimitPartition.GetFixedWindowLimiter("all:" + ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limits.RequestsPerMinute,
                Window = TimeSpan.FromMinutes(1),
            });
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRateLimiter();
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
