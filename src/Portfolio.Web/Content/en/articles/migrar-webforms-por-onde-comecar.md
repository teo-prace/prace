---
title: Migrating a WebForms system: where to start
summary: Before rewriting a single page, you need to know what the system actually does. A practical roadmap for the first step.
date: 2026-11-15
tags: WebForms, .NET, Modernization
draft: true
---

> **Sample draft.** Replace with your own text. While it has `draft: true`, this article only shows up on your machine.

## The most common mistake

Starting with a rewrite. A legacy system carries years of business rules that nobody documented.

## A 4-step roadmap

1. **Inventory**: how many pages, which dependencies, which .NET Framework version.
2. **Business rules map**: where they live (code-behind, stored procedures, JavaScript).
3. **Characterization tests**: capture current behavior before changing it.
4. **Slice-by-slice migration**: one feature at a time, with the old system running alongside.

```csharp
// Sample code snippet in an article
app.MapGet("/healthz", () => Results.Text("ok"));
```
