---
title: Demo case: modernizing a consórcio system without stopping it
summary: A complete legacy system, from VB6 to WebForms, moved to .NET 10 one screen at a time, with the old system running the whole time. The code is open.
date: 2026-11-22
tags: Case, WebForms, VB6, .NET 10, Blazor
draft: false
---

> **Fictional company, realistic system.** CeCons Administradora de Consórcios does not exist. The system
> was built to show the method on a legacy system with real weight: Brazilian consórcio rules (a group
> purchasing scheme regulated by Law 11.795/2008), 25 tables, 26 stored procedures, a batch, a customer
> portal, COM components, and the problems that pile up in twenty years of maintenance. All the code is on
> [GitHub](https://github.com/teo-prace/legacy-to-dotnet10-consorcio).

## The situation

A system from 2005 that never stopped growing: a SQL Server database with SQL 2000-style procedures, a VB6
desktop, a classic ASP customer portal with a VB6 component, a WebForms back office from 2011 and a VB.NET
batch from 2016. It works, the business runs on it every day, nobody knows all of it, and there isn't a
single automated test.

The usual request would be "rewrite it in .NET". I did the opposite: the old system stays up and stays the
reference, and the new one grows next to it, one screen at a time.

## First, find out what the system does

The assessment produced 36 findings. The most expensive one for a migration: **the same rule in several
places, giving different results**. Fines and interest were calculated in four places. The bank return
batch charged a fine to customers who paid on time after a holiday, and the portal did the same after a
weekend.

Instead of rewriting the rule from a document, I executed the old code and compared it with the new one:
333,960 late-fee cases. Every difference was categorized and taken to a decision; none was "fixed"
silently.

## The path

<ol class="roadmap">
<li class="done">
<span class="mono-label">01 · Assessment</span>
<h3>What the system really does</h3>
<p>Inventory, business rules map and 36 findings. The billing rules were compared with the old code, running.</p>
</li>
<li class="done">
<span class="mono-label">02 · Experiments</span>
<h3>Test every bet before depending on it</h3>
<p>Five experiments with automated tests: the new layer in front of the system, shared login, iframes, the UI technology, and two applications writing to the same database.</p>
</li>
<li class="done">
<span class="mono-label">03 · Decisions</span>
<h3>Six decisions, each with its evidence</h3>
<p>And a plan in phases, where every phase says which part of the old system is turned off.</p>
</li>
<li class="done">
<span class="mono-label">04 · Phase 0</span>
<h3>The new layer in front of the old system</h3>
<p>Every old screen goes through it unchanged. Along the way, it fixes a clickjacking exposure that existed today.</p>
</li>
<li class="done">
<span class="mono-label">05 · Phase 1</span>
<h3>The first new screen</h3>
<p>Installment search on .NET 10, at the same address and with the same look as the old one. Same rows, same order, compared by a test.</p>
</li>
<li class="done">
<span class="mono-label">06 · Phase 2</span>
<h3>Payment posting with a single rule</h3>
<p>Fines and interest from one rule, written through the old system's procedures, every rounding recorded.</p>
</li>
<li class="next">
<span class="mono-label">07 · Phase 3</span>
<h3>Bank return and customer portal</h3>
<p>The batch and the boleto copy move to .NET 10, and undue fees after holidays and weekends stop.</p>
</li>
<li class="next">
<span class="mono-label">08 · Phase 4</span>
<h3>All of billing on the new system</h3>
<p>The old billing screens, procedures and components are turned off.</p>
</li>
</ol>

## Both systems at the same time

The new screen deliberately uses the old layout: people don't have to relearn anything in the middle of the
migration. Both systems share the same database, and the new screen writes through the old system's own
procedures, so a payment posted on the new screen shows up immediately on the old one, and the write rule
stays in one place until the whole area moves.

Every new screen sits behind a switch. If something goes wrong in production, going back to the old screen
is a configuration change: no deploy, no restart.

## What the tests found along the way

| | |
|---|---|
| Old screens working behind the new layer | 26 of 26, 0.47 ms added per request |
| New boleto (Brazilian bank slip) compared with the old one | 0 differences over 2,287 installments |
| Problems found in the old system | a clickjacking exposure, a deadlock in the payment procedure, fines charged to customers who paid on time, a boleto bug due in 2050 |
| Bug in the new screen caught before it shipped | 1, a filter that returned 12 rows instead of 50, with no error |
| Automated tests | 48 |

## How I work

Assessment, a plan in small steps, and migration with the system running. Every important decision is
recorded with the evidence behind it. The repository has the whole path: the system as received, the
assessment, the experiments, the decisions and the new system.

I use AI tools to read and write code faster. The decisions, the validation and the tests are mine.

Have a system like this? [Let's talk](/en/contact).
