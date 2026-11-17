using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Portfolio.Web.Tests;

// Confere o limite de requisições por IP com números baixos (cada teste sobe o site do zero).
public class RateLimitTests
{
    private static HttpClient Client(int perMinute = 100, int contactPerWindow = 100) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b
                .UseSetting("RateLimit:RequestsPerMinute", perMinute.ToString())
                .UseSetting("RateLimit:ContactPerWindow", contactPerWindow.ToString()))
            .CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Pages_return_429_after_limit()
    {
        var client = Client(perMinute: 3);
        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/pt")).StatusCode);

        var blocked = await client.GetAsync("/pt");
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Contains("Too many requests", await blocked.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_check_is_never_limited()
    {
        var client = Client(perMinute: 1);
        for (var i = 0; i < 5; i++)
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/healthz")).StatusCode);
    }

    [Fact]
    public async Task Contact_form_has_its_own_tighter_limit()
    {
        var client = Client(contactPerWindow: 2);
        for (var i = 0; i < 2; i++)
            Assert.NotEqual(HttpStatusCode.TooManyRequests, (await client.PostAsync("/pt/contact", new FormUrlEncodedContent([]))).StatusCode);

        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsync("/pt/contact", new FormUrlEncodedContent([]))).StatusCode);
        // Navegar continua liberado: o limite do formulário é separado.
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/pt/contact")).StatusCode);
    }
}
