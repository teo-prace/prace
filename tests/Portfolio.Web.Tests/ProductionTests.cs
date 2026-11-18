using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Portfolio.Web.Tests;

// Sobe o site como em produção (sem o laboratório de marca) e confere o que o visitante recebe.
public class ProductionTests
{
    private static HttpClient Client() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseEnvironment("Production"))
            .CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Brand_lab_is_not_available()
    {
        var response = await Client().GetAsync("/brand");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Page_renders_only_one_header_logo_and_the_footer_logo()
    {
        var html = await Client().GetStringAsync("/en");

        // Header: só a variante configurada. Rodapé: sempre o horizontal.
        Assert.Equal(2, Regex.Matches(html, "data-v=\"").Count);
        Assert.Contains("brand-mark-fixed", html);
        Assert.DoesNotContain("data-brand-lab=\"", html);
    }

    [Fact]
    public async Task Pages_link_to_privacy_policy_and_declare_x_default()
    {
        var html = await Client().GetStringAsync("/pt/contact");
        Assert.Contains("href=\"/pt/privacy\"", html);
        Assert.Contains("hreflang=\"x-default\"", html);
    }
}
