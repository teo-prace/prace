using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Portfolio.Web.Content;

namespace Portfolio.Web.Components;

// Base das páginas com idioma na rota (/pt/..., /en/...).
public abstract class LocalizedPage : ComponentBase
{
    [Parameter] public string Lang { get; set; } = ContentService.DefaultLanguage;

    [Inject] protected ContentService ContentService { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] private IOptions<SiteOptions> SiteOptions { get; set; } = default!;

    protected SiteContent C { get; private set; } = default!;
    protected string Brand => SiteOptions.Value.Brand;

    protected override void OnParametersSet()
    {
        if (!ContentService.IsSupported(Lang))
            Nav.NotFound();

        C = ContentService.Get(Lang);
    }
}
