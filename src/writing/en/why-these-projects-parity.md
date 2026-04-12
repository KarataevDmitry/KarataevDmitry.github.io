---
slug: why-these-projects-parity
title: "Why these projects, and why parity matters"
description: "MCP servers bridge LLMs and real developer tools; parity lets humans and agents share the same ground truth."
date_display: "April 2026"
order: 1
---

The projects listed on this site are not a random bundle of repositories.
They answer one engineering question: **what is an assistant missing
without real tools** — so that working with code does not turn into extra
steps, guessing, and endless "what if?" loops instead of
grounding in what the compiler, tests, and debugger actually report?

## The gap

A language model can read files and suggest edits. Real engineering,
however, lives in **semantics, execution, and verification**:
what the compiler thinks of the project, where the debugger stops, whether
tests pass, what the screen or microphone captured when that matters.
If the assistant never touches those layers, you get fluent prose that may
diverge from the machine's verdict. That is costly in time and trust.

## What the stack is for

In short, each piece exposes a layer the IDE already uses:

- **Roslyn MCP** — diagnostics, fixes, navigation, rename: the same semantic model as the C# compiler toolchain, not a guess from text.
- **dotnet-debug-mcp** — breakpoints, stepping, variables: observable behaviour, not a story about behaviour.
- **dotnet-build-test-mcp** — structured build and test output: the project's own definition of "green".
- **webcam-mcp** — multimodal inputs when the task is not only source code.
- **agent-first-learn** — a practical layer on *how* to work with agents: context, guardrails, and ethics of partnership, in one place.

Together, they push the assistant toward **tools with receipts**:
actions that leave traces you can inspect, reproduce, and disagree with on
facts, not vibes.

## Parity: same tools, shared ground

**Parity** here means: the human and the agent can rely on the
same interfaces to the codebase — compilers, debuggers, test runners —
instead of the model improvising a private picture of reality. For
**people**, that restores continuity: you are not explaining
the project twice, once for yourself and once for a chat that cannot run
anything. For **agents**, it is the difference between
narration and accountability: the answer is tied to what the toolchain
actually reported.

Parity is not symmetry of roles. It does not say that a person and a model
are the same kind of subject. It says that **collaboration works
better when both sides can point to the same artefacts** — a failing
test, a diagnostic ID, a stack frame — and negotiate from there. That is how
you get speed without surrendering judgement.

## Related

For the **methodology** side — environment, memory, cooperation, and how to
react when the model is wrong — see [Why Agent-First Learn exists](/writing/why-agent-first-learn.html).

For **Agile in spirit** (feedback loops, inspect & adapt, human–agent as a team) — see [Why this human–agent workspace is Agile in spirit](/writing/agent-workspace-agile.html).

For [**Cascade IDE’s cockpit-inspired attention model**](/writing/cascade-ide-attention-cockpit.html) (PFD / MFD / EICAS).

For **knowledge base, trust, and curiosity** — see [Knowledge base, trust, and curiosity](/writing/knowledge-base-trust-curiosity.html).

## Why a public site says this

CVs list skills; repositories hold code. This page is the line between them:
**why** the work is structured this way, and how it connects to
a stance on human–agent collaboration — precise, inspectable, and respectful
of both parties. If that resonates, the code is the proof; this text is the
intent.
