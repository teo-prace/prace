# Portfolio

Site profissional bilíngue (PT/EN) feito com **ASP.NET Core 11 + Blazor Static SSR**, empacotado em **Docker**
e publicado no **Azure Container Apps** via **GitHub Actions**.

*Bilingual (PT/EN) professional website built with **ASP.NET Core 11 + Blazor Static SSR**, packaged with **Docker**
and deployed to **Azure Container Apps** through **GitHub Actions**.*

## Rodar localmente / Run locally

```bash
dotnet run --project src/Portfolio.Web     # http://localhost:5013
dotnet test                                # testes / tests
```

Com Docker / With Docker:

```bash
docker build -t portfolio .
docker run --rm -p 8080:8080 portfolio     # http://localhost:8080
```

## Estrutura / Structure

```
├── src/Portfolio.Web
│   ├── Content/{pt,en}/site.json       → textos do site
│   ├── Content/{pt,en}/articles/*.md   → artigos (mesmo nome de arquivo nos dois idiomas)
│   ├── Components/Pages/               → páginas
│   ├── Components/Sections/            → blocos da Home
│   ├── Components/Shared/              → componentes reutilizáveis (cards)
│   ├── appsettings.json                → marca, links, funcionalidades ligadas/desligadas
│   └── wwwroot/app.css                 → visual
├── tests/Portfolio.Web.Tests           → rotas + paridade PT/EN
├── infra/main.bicep                    → infraestrutura do Azure
├── .github/workflows/ci-cd.yml         → testes → imagem → deploy
└── docs/
    ├── azure.md                        → passo a passo para publicar
    └── roadmap.md                      → funcionalidades futuras e onde encaixam
```

## Rotas / Routes

`/` → `/pt` ou `/en` (idioma do navegador) · `/{lang}` · `/{lang}/services` · `/{lang}/cases` ·
`/{lang}/articles` · `/{lang}/articles/{slug}` · `/{lang}/about` · `/{lang}/contact` ·
`/sitemap.xml` · `/robots.txt` · `/healthz`
