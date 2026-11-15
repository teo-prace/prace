using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Portfolio.Web.Tests;

// Sobe o site em memória e confere que as páginas respondem.
public class RouteTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private HttpClient Client() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Theory]
    [InlineData("/pt")]
    [InlineData("/en")]
    [InlineData("/pt/services")]
    [InlineData("/en/services")]
    [InlineData("/pt/cases")]
    [InlineData("/en/cases")]
    [InlineData("/pt/about")]
    [InlineData("/en/about")]
    [InlineData("/pt/contact")]
    [InlineData("/en/contact")]
    [InlineData("/pt/articles")]
    [InlineData("/en/articles")]
    [InlineData("/healthz")]
    [InlineData("/sitemap.xml")]
    [InlineData("/robots.txt")]
    [InlineData("/brand")]
    public async Task Page_responds_ok(string url)
    {
        var response = await Client().GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/xx")]
    [InlineData("/xx/services")]
    [InlineData("/pt/does-not-exist")]
    [InlineData("/pt/articles/does-not-exist")]
    public async Task Unknown_page_returns_404(string url)
    {
        var response = await Client().GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("pt-BR,pt;q=0.9", "/pt")]
    [InlineData("en-US,en;q=0.9", "/en")]
    [InlineData("es-ES", "/en")]
    public async Task Root_redirects_by_browser_language(string acceptLanguage, string expected)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Accept-Language", acceptLanguage);
        var response = await Client().SendAsync(request);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expected, response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Html_lang_matches_route()
    {
        var pt = await Client().GetStringAsync("/pt");
        var en = await Client().GetStringAsync("/en");
        Assert.Contains("<html lang=\"pt-BR\"", pt);
        Assert.Contains("<html lang=\"en\"", en);
    }
}
