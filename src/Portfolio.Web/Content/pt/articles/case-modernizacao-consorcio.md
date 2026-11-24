---
title: Case demonstrativo: modernizando um sistema de consórcio sem parar a operação
summary: Um sistema legado completo, de VB6 a WebForms, migrado para .NET 10 uma tela por vez, com o sistema antigo no ar o tempo todo. O código está aberto.
date: 2026-11-22
tags: Case, WebForms, VB6, .NET 10, Blazor
draft: false
---

> **Empresa fictícia, sistema realista.** A CeCons Administradora de Consórcios não existe. Empresa, dados e
> regras fictícios, criados para demonstração a partir da legislação pública. O sistema foi
> construído para mostrar o método num legado com peso de verdade: regras de consórcio da Lei 11.795/2008,
> 25 tabelas, 26 procedures, batch, portal do consorciado, componentes COM e os problemas que se acumulam
> em vinte anos de manutenção. Todo o código está no
> [GitHub](https://github.com/teo-prace/legacy-to-dotnet10-consorcio).

## O cenário

Um sistema de 2005 que nunca parou de crescer: banco SQL Server com procedures no estilo SQL 2000, desktop
em VB6, portal do consorciado em ASP clássico com um componente VB6, retaguarda em WebForms de 2011 e um
batch em VB.NET de 2016. Funciona, o negócio roda nele todo dia, ninguém conhece tudo e não há nenhum teste
automatizado.

O pedido de sempre seria "reescreve em .NET". Eu fiz o contrário: o sistema antigo continua no ar e
continua sendo a referência, e o novo cresce ao lado dele, uma tela por vez.

## Primeiro, entender o que o sistema faz

O diagnóstico levantou 36 achados. O mais caro para uma migração: **a mesma regra em vários lugares, com
resultados diferentes**. Multa e juros eram calculados em quatro pontos do sistema. O batch do retorno
bancário cobrava multa de quem pagou em dia depois de um feriado, e o portal fazia o mesmo depois de um fim
de semana.

Em vez de reescrever a regra a partir de um documento, executei o código antigo e comparei com o novo:
333.960 casos de multa e juros. Toda diferença foi classificada e levada para decisão, nenhuma foi
"corrigida" em silêncio.

## O caminho

<ol class="roadmap">
<li class="done">
<span class="mono-label">01 · Diagnóstico</span>
<h3>O que o sistema faz de verdade</h3>
<p>Inventário, mapa de regras de negócio e 36 achados. As regras de cobrança foram comparadas com o código antigo executando.</p>
</li>
<li class="done">
<span class="mono-label">02 · Experimentos</span>
<h3>Testar cada aposta antes de depender dela</h3>
<p>Cinco experimentos com testes automatizados: a camada nova na frente do sistema, o login compartilhado, iframe, a tecnologia das telas e duas aplicações gravando no mesmo banco.</p>
</li>
<li class="done">
<span class="mono-label">03 · Decisões</span>
<h3>Seis decisões, cada uma com a sua evidência</h3>
<p>E um plano em fases, em que cada fase diz qual parte do sistema antigo é desligada.</p>
</li>
<li class="done">
<span class="mono-label">04 · Fase 0</span>
<h3>A camada nova na frente do sistema antigo</h3>
<p>Todas as telas antigas passam por ela sem mudança. De quebra, corrige uma exposição a clickjacking que existia hoje.</p>
</li>
<li class="done">
<span class="mono-label">05 · Fase 1</span>
<h3>A primeira tela nova</h3>
<p>Consulta de parcelas em .NET 10, no mesmo endereço e com o mesmo visual da antiga. Mesmas linhas, mesma ordem, comparadas por teste.</p>
</li>
<li class="done">
<span class="mono-label">06 · Fase 2</span>
<h3>Baixa de parcelas com uma regra só</h3>
<p>Multa e juros calculados por uma única regra, gravação pelas procedures do sistema antigo e cada arredondamento registrado.</p>
</li>
<li class="next">
<span class="mono-label">07 · Fase 3</span>
<h3>Retorno bancário e portal</h3>
<p>O batch e a 2ª via do boleto passam para o .NET 10, e acabam as cobranças indevidas depois de feriados e fins de semana.</p>
</li>
<li class="next">
<span class="mono-label">08 · Fase 4</span>
<h3>Cobrança inteira no sistema novo</h3>
<p>As telas, procedures e componentes antigos de cobrança são desligados.</p>
</li>
</ol>

## Os dois sistemas ao mesmo tempo

A tela nova usa o mesmo layout da antiga de propósito: quem usa o sistema não precisa reaprender nada no meio
da migração. Os dois sistemas convivem no mesmo banco, e a tela nova grava pelas mesmas procedures do antigo,
então uma baixa feita na tela nova aparece na hora na tela antiga, e a regra de gravação continua num lugar
só até a área inteira migrar.

Cada tela nova fica atrás de uma chave. Se algo der errado em produção, voltar para a tela antiga é mudar uma
configuração, sem publicar nada e sem reiniciar o sistema.

## O que os testes encontraram no caminho

| | |
|---|---|
| Telas antigas funcionando por trás da camada nova | 26 de 26, com 0,47 ms a mais por requisição |
| Boleto novo comparado com o antigo | 0 diferenças em 2.287 parcelas |
| Problemas encontrados no sistema antigo | exposição a clickjacking, um deadlock na procedure de baixa, multa cobrada de quem pagou em dia, um bug no boleto que só aparece em 2050 |
| Bug da tela nova pego antes de ir ao ar | 1, um filtro que devolvia 12 linhas em vez de 50, sem erro nenhum |
| Testes automatizados | 48 |

## Como eu trabalho

Diagnóstico, plano em etapas pequenas e migração com o sistema rodando. Cada decisão importante ficou
registrada com a evidência que a sustenta. O repositório tem o caminho inteiro: o sistema recebido, o
diagnóstico, os experimentos, as decisões e o sistema novo.

Uso ferramentas de IA para ler e escrever código mais rápido. As decisões, a validação e os testes são meus.

Tem um sistema parecido? [Vamos conversar](/pt/contact).
