---
title: Migrar um sistema WebForms: por onde começar
summary: Antes de reescrever qualquer tela, é preciso saber o que o sistema realmente faz. Um roteiro prático para o primeiro passo.
date: 2026-11-15
tags: WebForms, .NET, Modernização
draft: true
---

> **Rascunho de exemplo.** Substitua pelo seu texto. Enquanto tiver `draft: true`, este artigo só aparece na sua máquina.

## O erro mais comum

Começar pela reescrita. O sistema legado carrega anos de regras de negócio que ninguém documentou.

## Um roteiro em 4 passos

1. **Inventário**: quantas páginas, quais dependências, qual versão do .NET Framework.
2. **Mapa de regras de negócio**: onde estão (code-behind, procedures, JavaScript).
3. **Testes de caracterização**: registrar o comportamento atual antes de mudar.
4. **Migração por fatias**: uma funcionalidade por vez, com o sistema antigo rodando ao lado.

```csharp
// Exemplo de trecho de código no artigo
app.MapGet("/healthz", () => Results.Text("ok"));
```
