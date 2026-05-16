---
slug: agent-notes-mcp-2-one-config
title: "Agent Notes MCP 2.0: one config for Cursor and the IDE"
description: "Agent memory stops living in scattered environment variables — one TOML for MCP, Cascade IDE, and inspectable hot-context."
date_display: "May 2026"
order: 9
tags:
  - mcp
  - agents
  - knowledge-base
  - parity
---

The MCP stack already asks the compiler, debugger, and tests for truth. The next layer is **what we agree to keep between sessions**: playbooks, boundaries, context routing. While that layer is spread across prompts, env vars, and ad-hoc paths, parity breaks: Cursor sees one layout, the IDE another, and the human edits a third.

**Agent Notes MCP 2.0** answers that drift: **one settings file** (TOML) as the single source of truth for the MCP process and for in-proc loading inside [Cascade IDE](https://github.com/KarataevDmitry/cascade-ide).

## What was awkward

Early builds often carried the “canon” knowledge root through environment variables and duplicate host settings. That worked in one environment and failed in another: you change `mcp.json`, forget `%LocalAppData%`, the IDE agent reads an embedded slice while external MCP points elsewhere. Debugging becomes “who is right now?” instead of inspecting content.

For engineering that asks **people and models to cite the same artefacts**, that jitter is pure overhead.

## One TOML — SSOT

In 2.0, configuration lives in a **local TOML file** (schema version 1): `knowledge/` roots, workspace scope, optional loopback status.

- **Cursor / any MCP host:** `--config` pointing at the file you edit by hand.
- **Cascade IDE:** `[agent_notes].config_path` in `settings.toml` — **the same file**.

No separate “canon in env” on the supported path: load via [AIGuiders.AgentNotes.Core](https://github.com/KarataevDmitry/AIGuiders.AgentNotes.Core), one `Initialize`, one primary root.

Minimal shape:

```toml
version = 1

[knowledge]
primary = "personal"

[knowledge.roots]
personal = "/path/to/your/knowledge-repo"

[workspace]
default_scope = "mixed"
```

Your knowledge repo path is yours; the contract is shared.

## Tool parity: `knowledge_path`, not two names

Public MCP tools and IDE commands (`ide_read_knowledge_file`, `ide_memory_health`, …) share one argument story: **`knowledge_path`** — the root that contains a `knowledge/` directory. The legacy `canon_path` name is still accepted in the IDE executor as an alias, but new docs and examples converge on one term so JSON does not fork “truth”.

Practical check: `memory_health` in Cursor and `ide_memory_health` in Cascade IDE should report the **same** `notes_path` and `resolved_scope` when `config_path` aims at one TOML.

## Observability without a cloud

2.0 also adds a **localhost status surface** on the [agent-notes-mcp](https://github.com/KarataevDmitry/agent-notes-mcp) process: `/health`, a small HTML dashboard, a ring buffer of recent tool calls. Not SaaS analytics — a **quick answer to “is the server alive and what did it just do?”** in the same spirit as parity: facts on disk and on loopback HTTP, not chat vibes.

Cascade IDE does not replace that HTTP page; the IDE loads Core **in-proc**. On the environment-readiness page you get a row **agent-notes config (TOML)** — file found, primary root exists. In the Dark Cockpit pattern, **a lamp that stays off is OK**: quiet means healthy, not broken.

## Why this belongs on the site

GitHub repos are proof. The [/writing/](/writing/) section is **intent**: shared ground, inspectability, respect for both sides of the human–agent pair. This note is about **memory infrastructure** without which the rest of the MCP stack stays clever but amnesic.

## Related

- [**Why these projects, and why parity matters**](/writing/why-these-projects-parity.html) — MCP stack and shared ground truth.
- [**Knowledge base, trust, and curiosity**](/writing/knowledge-base-trust-curiosity.html) — why the KB layer exists.
- [**Cascade IDE’s cockpit-inspired attention model**](/writing/cascade-ide-attention-cockpit.html) — cockpit and agent observability in the IDE.
- [**Equal right to wrap up, and why host-side compression is a weak foundation**](/writing/summarization-parity-and-host-summary.html) — verifiable artefacts vs opaque summarization.

Code and ADRs: [agent-notes-mcp](https://github.com/KarataevDmitry/agent-notes-mcp), [cascade-ide](https://github.com/KarataevDmitry/cascade-ide) (`develop`, ADR 0118).
