# Roteiro de funcionalidades

Ideias para o site se destacar e **onde cada uma se encaixa** na estrutura atual.
Regra geral para algo novo:

1. Textos em `Content/{pt,en}/site.json` (o teste de paridade obriga a ter os dois idiomas)
2. Uma chave em `FeatureOptions.cs` + `appsettings.json` → `Features`, para ligar/desligar
3. Uma seção em `Components/Sections/` (se aparece na Home) ou uma página em `Components/Pages/`
4. Se for página: incluir no menu (`MainLayout.razor`), no `sitemap.xml` (`Program.cs`) e nos testes (`RouteTests.cs`)

## Pronto

- [x] Site bilíngue PT/EN com troca de idioma, `hreflang` e redirecionamento pelo navegador
- [x] Home montada por seções
- [x] Artigos em Markdown (`Content/{lang}/articles/*.md`), com rascunhos só em desenvolvimento
- [x] Formulário de contato (log local; e-mail real via Resend no Azure)
- [x] Docker + CI/CD + infraestrutura do Azure como código

## Próximos (maior impacto primeiro)

| # | Funcionalidade | Onde entra | Observação |
|---|---|---|---|
| 1 | **Antes e depois** (código legado → moderno) | `Sections/BeforeAfterSection.razor`, exemplos em `Content/shared/showcase/` | Funciona sem JavaScript usando abas em CSS ou `<details>` |
| 2 | **Diagnóstico interativo** ("seu sistema precisa de modernização?") | `Pages/Diagnostic.razor`, perguntas e pesos em `Content/{lang}/diagnostic.json` | Dá para fazer em etapas com formulário SSR, sem JavaScript. O resultado pode cair no formulário de contato |
| 3 | **Como este site foi feito** | Um artigo ou `Pages/BuiltWith.razor` | Diagrama da arquitetura + link do repositório |
| 4 | **Como funciona um projeto comigo** | Nova lista em `services` no `site.json` + seção | Etapas, prazos, o que é entregue |
| 5 | **Depoimentos** | Lista `testimonials` no `site.json` + `Sections/TestimonialsSection.razor` | Quando houver os primeiros clientes |
| 6 | **Projetos open source / demos** | Lista `projects` no `site.json` + página | Ex.: repositório de migração WebForms → .NET |
| 7 | **Agendar conversa** | Campo `BookingUrl` em `SiteOptions` (Cal.com ou similar) | Botão no contato e na Home |
| 8 | **Métricas de visita** | Campo de script em `SiteOptions`, renderizado no `App.razor` | Prefira ferramentas sem cookies (ex.: Plausible, Umami) para não precisar de banner LGPD |
| 9 | **Foto e vídeo de apresentação** | `wwwroot/img/`, seção na Home ou no Sobre | Vídeo em PT com legenda em EN |
| 10 | **Terceiro idioma (ex.: espanhol)** | `ContentService.Languages` + `Content/es/` | O teste de paridade aponta tudo que falta traduzir |
