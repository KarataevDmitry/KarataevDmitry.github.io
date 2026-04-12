---
slug: cascade-ide-attention-cockpit
title: "Why Cascade IDE borrows a cockpit attention model"
description: "Signal overload, context switches, and a deliberate hierarchy of zones — PFD, MFD, EICAS — so the editor stays primary and the agent stays observable."
date_display: "April 2026"
order: 4
---

**Cascade IDE** is a .NET-focused IDE meant to be **driven by an agent through MCP**
while keeping **humans and tools in the same observable loop**. One of its design
threads is explicit: **where your eyes should go, and when** — borrowed from
how cockpits manage attention in aviation.

This text is a **conceptual sketch**. A fuller specification exists **inside the
Cascade IDE repository** as an architecture decision record (working name **ADR 0021**,
PFD / MFD / cockpit attention model). The repository is **not public yet**, so this
page is the readable entry point. The product is **in active development**; details
will move as implementation catches up.

## The tax: context switches

A developer juggles code, build output, tests, git, diagnostics, and an agent
that may be half a screen away in another app. The productivity hit is not only
Alt+Tab — it is **losing the thread**: which channel mattered, what failed last,
whether you and the model are looking at the same truth.

An IDE that wants **agent-first** work cannot treat “more panels” as neutral.
Without a **priority model**, the UI drifts to two bad extremes: everything
hidden (no feedback) or everything visible (banner blindness).

## Cockpit metaphors are not decoration

Airliners split instruments by **role in attention management**: primary flight
context, secondary switchable views, a **consolidated alerting** channel. That
is not nostalgia — it is a response to **limited attention and high cost of
error**.

Cascade maps that idea to software work: **stable anchors** for where you look,
what is **secondary by conscious choice**, and what **escalates** when something
is wrong — instead of every subsystem fighting for the same z-order.

## A very short map of the zones

In plain language:

- **Forward / editor** — the object of work; this is where time and space
  dominate. In-editor overlays (**HUD**-style hints) stay **inside** this anchor;
  they do not become a fourth competing “window soul.”
- **PFD-shaped region** — “flight context”: solution tree, where you are in the
  workspace, task focus, compact signals that you are still oriented.
- **MFD-shaped region** — deliberately secondary: git, long logs, browser,
  full agent trace, terminal — things you **open on purpose**, not permanent
  competitors to the editor.
- **EICAS-shaped channel** — **alerts and prioritization** (warning / caution /
  advisory style), not “a third column for fun.” Placement can vary by preset;
  the **role** is what matters.

The important move in that specification is separating **spatial anchors** (where regions
sit) from **attention policy** (Focus / Balanced / Power, escalation, what
counts as loud). Mixing those layers in docs or code is how products quietly
return to chaos.

## “Everything in one place” without embedding the whole internet

The north-star wording is **one coherent scene**, not “ship every chat client
inside the binary.” Ten different chat UIs inside the IDE mostly add **login
surfaces and notification noise**. The cockpit model is there to keep a
**sustainable contour**: predictable places for truth, bridges to external hosts
when needed, and **no plugin free-for-all** over the editor without declaring
which anchor it belongs to.

## Why this pairs with MCP and parity

The same philosophy that pushes **Roslyn, debuggers, and tests** through MCP is
the philosophy that says the **IDE surface must not lie** about state. An
attention model is how you keep that honesty **legible** — for a person and for
an agent trying to stay aligned.

## Related

- [**Parity with the toolchain**](/writing/why-these-projects-parity.html) — MCP, shared ground truth
- [Why Agent-First Learn exists](/writing/why-agent-first-learn.html) — methodology
- [Why this human–agent workspace is Agile in spirit](/writing/agent-workspace-agile.html) — Agile in spirit
- [Knowledge base, trust, and curiosity](/writing/knowledge-base-trust-curiosity.html) — knowledge base, trust, curiosity
- [Attention, friction, and neurodivergence in the IDE](/writing/attention-contour-neurodivergence.html) — attention, friction, neurodivergence
