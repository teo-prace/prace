namespace Portfolio.Web.Content;

// Modelo dos textos do site. Os valores ficam em Content/{pt,en}/site.json.
// Nos textos, *assim* vira itálico serifado e **assim** vira negrito (veja Rich.cs).
public sealed class SiteContent
{
    public NavContent Nav { get; init; } = new();
    public HomeContent Home { get; init; } = new();
    public ServicesContent Services { get; init; } = new();
    public CasesContent Cases { get; init; } = new();
    public TestimonialsContent Testimonials { get; init; } = new();
    public AboutContent About { get; init; } = new();
    public ContactContent Contact { get; init; } = new();
    public ArticlesContent Articles { get; init; } = new();
    public PrivacyContent Privacy { get; init; } = new();
    public FooterContent Footer { get; init; } = new();
}

public sealed class NavContent
{
    public string Home { get; init; } = "";
    public string Services { get; init; } = "";
    public string Process { get; init; } = "";
    public string Cases { get; init; } = "";
    public string Articles { get; init; } = "";
    public string About { get; init; } = "";
    public string Contact { get; init; } = "";
    public string Privacy { get; init; } = "";
    public string CtaSecondary { get; init; } = "";
    public string CtaPrimary { get; init; } = "";
    public string SwitchLanguage { get; init; } = "";
    public string ThemeToggle { get; init; } = "";
    public string Menu { get; init; } = "";
}

public sealed class HomeContent
{
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Breadcrumb { get; init; } = "";
    public string Headline { get; init; } = "";
    public string Lead { get; init; } = "";
    public string CtaPrimary { get; init; } = "";
    public string CtaSecondary { get; init; } = "";
    public AuditContent Audit { get; init; } = new();
    public ChangesContent Changes { get; init; } = new();
    public string ServicesTag { get; init; } = "";
    public string ServicesTitle { get; init; } = "";
    public string ServicesText { get; init; } = "";
    public ProcessContent Process { get; init; } = new();
    public string FeaturedCaseTag { get; init; } = "";
    public string FeaturedCaseTitle { get; init; } = "";
    public string AllCases { get; init; } = "";
    public string LatestArticlesTag { get; init; } = "";
    public string LatestArticlesTitle { get; init; } = "";
    public string ClosingTag { get; init; } = "";
    public string ClosingTitle { get; init; } = "";
    public string ClosingText { get; init; } = "";
}

public sealed class AuditContent
{
    public string Title { get; init; } = "";
    public string Counter { get; init; } = "";
    public List<AuditGroup> Groups { get; init; } = [];
    public List<string> Footer { get; init; } = [];
}

public sealed class AuditGroup
{
    public string Name { get; init; } = "";
    public List<AuditRow> Rows { get; init; } = [];
}

public sealed class AuditRow
{
    public string Name { get; init; } = "";
    public string Detail { get; init; } = "";
    public string Value { get; init; } = "";
}

public sealed class ChangesContent
{
    public string Tag { get; init; } = "";
    public string Title { get; init; } = "";
    public string Text { get; init; } = "";
    public List<ChangeItem> Items { get; init; } = [];
}

public sealed class ChangeItem
{
    public string From { get; init; } = "";
    public string To { get; init; } = "";
    public string Text { get; init; } = "";
}

public sealed class ProcessContent
{
    public string Tag { get; init; } = "";
    public string Title { get; init; } = "";
    public List<ProcessStep> Items { get; init; } = [];
}

public sealed class ProcessStep
{
    public string Title { get; init; } = "";
    public string Text { get; init; } = "";
}

public sealed class ServicesContent
{
    public string Title { get; init; } = "";
    public string Intro { get; init; } = "";
    public List<ServiceItem> Items { get; init; } = [];
}

public sealed class ServiceItem
{
    // Âncora na página de serviços (/pt/services#id). Cada idioma usa um id no próprio idioma.
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Summary { get; init; } = "";
    public List<string> Bullets { get; init; } = [];
}

public sealed class CasesContent
{
    public string Title { get; init; } = "";
    public string Intro { get; init; } = "";
    public CaseLabels Labels { get; init; } = new();
    public List<CaseItem> Items { get; init; } = [];
}

public sealed class CaseLabels
{
    public string Context { get; init; } = "";
    public string Challenge { get; init; } = "";
    public string Solution { get; init; } = "";
    public string Result { get; init; } = "";
}

public sealed class CaseItem
{
    public string Title { get; init; } = "";
    public string Client { get; init; } = "";
    public string Context { get; init; } = "";
    public string Challenge { get; init; } = "";
    public string Solution { get; init; } = "";
    public string Result { get; init; } = "";
    public List<string> Tags { get; init; } = [];
    // Opcionais: link para saber mais (artigo, repositório). Os dois juntos ou nenhum.
    public string? Link { get; init; }
    public string? LinkText { get; init; }
}

// Só aparece no site quando houver itens. Use apenas depoimentos reais, com autorização.
public sealed class TestimonialsContent
{
    public string Tag { get; init; } = "";
    public string Title { get; init; } = "";
    public List<Testimonial> Items { get; init; } = [];
}

public sealed class Testimonial
{
    public string Quote { get; init; } = "";
    public string Author { get; init; } = "";
    public string Role { get; init; } = "";
}

public sealed class AboutContent
{
    public string Title { get; init; } = "";
    public string Lead { get; init; } = "";
    // Texto alternativo do retrato (leitores de tela).
    public string PortraitAlt { get; init; } = "";
    public List<string> Paragraphs { get; init; } = [];
    public string StackTag { get; init; } = "";
    public string StackTitle { get; init; } = "";
    public List<StackGroup> Stack { get; init; } = [];
    public string HowTag { get; init; } = "";
    public string HowTitle { get; init; } = "";
    public List<string> How { get; init; } = [];
}

public sealed class StackGroup
{
    public string Name { get; init; } = "";
    public List<string> Items { get; init; } = [];
}

public sealed class ContactContent
{
    public string Title { get; init; } = "";
    public string Intro { get; init; } = "";
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string Message { get; init; } = "";
    public string Send { get; init; } = "";
    public string Success { get; init; } = "";
    public string Failure { get; init; } = "";
    public string Required { get; init; } = "";
    public string InvalidEmail { get; init; } = "";
    public string DirectTitle { get; init; } = "";
    // Frase abaixo do botão; o link para a política de privacidade vem logo depois (Nav.Privacy).
    public string PrivacyNote { get; init; } = "";
}

// Política de privacidade (LGPD). Revise ao mudar o que o site coleta ou os serviços usados.
public sealed class PrivacyContent
{
    public string Title { get; init; } = "";
    public string Lead { get; init; } = "";
    public string Updated { get; init; } = "";
    public List<PrivacySection> Sections { get; init; } = [];
}

public sealed class PrivacySection
{
    public string Title { get; init; } = "";
    public string Text { get; init; } = "";
}

public sealed class ArticlesContent
{
    public string Title { get; init; } = "";
    public string Intro { get; init; } = "";
    public string Empty { get; init; } = "";
    public string ReadMore { get; init; } = "";
    public string BackToList { get; init; } = "";
    public string Draft { get; init; } = "";
}

public sealed class FooterContent
{
    public string Tagline { get; init; } = "";
    // Discreto, na linha do copyright.
    public string Location { get; init; } = "";
}
