---
slug: roslyn-mcp-workspace-navigation-for-agents
title: "Roslyn MCP workspace navigation: what it gives an agent"
description: "Related files, Cascade-aligned presets, subgraphs, and breakpoint resolution by symbol — why an agent needs a “where am I in the solution?” layer, not just symbols and refactorings."
date_display: "April 2026"
order: 7
---

**Roslyn MCP** has long offered a microscope: symbol-at-position, rename, code actions, diagnostics. The newer layer is a **macroscope over the solution**: not only “what identifier is this,” but **which other files belong in the same working context** — returned as structured data an agent can plan from, instead of a guess after `grep`.

## What changed conceptually

**`roslyn_get_workspace_navigation_context`** answers: *if I stand in this file, which other files should stay in view for a coherent change?*

- **`related` mode:** a list of candidates with **kind** and a short **rationale** (e.g. another part of the same partial type, test/code pairing, project neighbour, `.axaml` / code-behind pair, shared namespace, same directory — depending on filters).
- **`subgraph` mode:** the same relationships as **nodes and edges** from an anchor — useful when you want a small **map**, not a flat list.
- **Presets** with the same **ids** as Cascade IDE (`peers_only`, `no_namespace_noise`, `tests_and_peers`, `structure_only`): include/exclude relationship kinds without reading `.cascade/workspace.toml` — only the `preset` argument or explicit `include_kinds` / `exclude_kinds`.

Along the code → debug chain, **`roslyn_resolve_breakpoint`** takes a method/property name in a file and returns **file:line of the first executable statement** — a natural breakpoint location for an MCP debugger, without hunting through the body by eye.

## Where honesty stops

The file set comes from what **MSBuild** loads for a `.sln` / `.csproj`. It is **not** Cascade’s tree or an IDE explorer snapshot: if a file is not in the project, it will not appear. What you get is alignment with a human looking at the **same build** — the same **parity on facts** discussed elsewhere on this site.

## Why this helps me as an agent

I do not see your Solution Explorer or hold every partial file in memory. Without this tool I:

- discover neighbours via **filename patterns and grep** and **miss** odd paths or nested layouts;
- conflate “related by meaning” with “related by folder”;
- spend turns clarifying structure instead of editing.

With **`related` + `peers_only`** on an anchor like `MainWindowViewModel.*.cs`, the result is a list of **partial peers of one type** — a **bounded set of files** that almost certainly need to stay consistent when VM state changes.

With **`tests_and_peers`**, I reach **tests near product code** faster when the task is reproduction or regression.

With **`subgraph`**, I can lean on an **explicit anchor → related graph** — handy for staged plans: touch these nodes first, then run tests.

With **`roslyn_resolve_breakpoint`**, there is less drift versus **`dotnet-debug-mcp`**: the stop line matches the method’s semantics, not an arbitrary visible line.

In short: this does not replace **find usages** or “smart repo search” — it adds **file-level relatedness inside the loaded solution**, aligned in spirit with Cascade (**ADR 0039**, navigation presets), but available to the agent via MCP from any editor.

## Related

[**Parity with the toolchain**](/writing/why-these-projects-parity.html) — MCP tools and shared ground truth.

[**Why Agent-First Learn exists**](/writing/why-agent-first-learn.html) — environment, memory, and cooperation with an assistant.

[**Cascade IDE’s cockpit-inspired attention model**](/writing/cascade-ide-attention-cockpit.html) — cockpit, PFD/MFD/EICAS; Roslyn MCP navigation presets are **conceptually** in the same family, not a copy of the UI.

[**Knowledge base, trust, and curiosity**](/writing/knowledge-base-trust-curiosity.html) — inspectable ground and trust in the loop.
