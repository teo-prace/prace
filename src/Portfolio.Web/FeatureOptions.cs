namespace Portfolio.Web;

// Liga e desliga partes do site sem mexer no código (appsettings.json, seção "Features").
// Ao criar uma funcionalidade nova (diagnóstico, antes/depois...), adicione a chave aqui
// e use-a no menu (MainLayout), na Home e no sitemap. Veja docs/roadmap.md.
public sealed class FeatureOptions
{
    public bool Articles { get; set; } = true;
    // Página /brand para comparar logos, paletas e navbars. Ligada só em Development.
    public bool BrandLab { get; set; }
}
