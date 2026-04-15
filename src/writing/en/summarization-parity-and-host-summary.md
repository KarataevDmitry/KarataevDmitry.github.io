---
slug: summarization-parity-and-host-summary
title: "Equal right to summarize, and why host-side compression is a weak foundation"
description: "Explicit summaries and exports are a shared discipline; opaque host compression replaces the verifiable artifact and breaks parity."
date_display: "April 2026"
order: 6
tags:
  - agents
  - parity
  - cursor
  - knowledge-base
---

A long chat with an assistant eventually hits a **context budget**. That raises a simple question: **what counts as “saved” from the thread** — and **who gets to fix it**. This note contrasts two answers: **explicit summarization by agreement** versus **compression performed by the host** (here: Cursor and similar IDEs). The first can be made honest; the second is almost always **opaque** and sits poorly with human–agent **parity**.

## Two meanings of “summarization”

**Summarization as agreement** means participants **deliberately** decide to close the loop: what was decided, what stayed open, what comes next. It can be **checked**: there is a shared text, an export, or a pointer to a file. Either the human or the agent can propose it — a **symmetric right**, not a privilege.

**Summarization as host behavior** is different: the model or host **compresses history** so the session can continue. In the UI it may look like a short recap instead of the full thread. That recap often has **no stable path on disk** and is harder to **quote** and **dispute line by line** than the raw log or a agreed markdown.

Conflating the two means confusing **a collaboration tool** with a **platform tradeoff**.

## Why parity needs explicit closure

Parity here is not abstract “equal votes”; it is **shared reliance on verifiable artifacts**: tests, logs, repo files, a readable chat export. When closure is **explicitly agreed**, you can separate **what we decided** from **what the model remembers** having been said.

If closure is **host compression**, then:

- the **source of truth** moves into a contour you cannot fully audit;
- **what matters** (what is kept vs dropped) follows host heuristics, not your engineering intent;
- the **agent** in a new session may receive a **mediated** version of the past, with drift and lost nuance.

Parity in facts breaks where the fact is **not** recovered from a file but from **compressed representation**.

## Why “Cursor compression” is a bad primary plan

This is not a rant at the product: every host has a **context budget**, and long sessions without compression do not exist. The issue is **relying on that compression as memory**.

It is **opaque**: you do not control which phrases vanish and which survive the “short recap for the model.” It is **not** your signed document — not a commit, not a note to yourself, not a text agreed with the agent. It does **not** replace a file export when you need **citable** and **reproducible** history.

So: host compression is an **infrastructure risk** you may tolerate; it is **not** a trusted canon for long arcs and team agreements.

## A practical pipeline

1. **Raw transcript** on disk (`*.jsonl` under `agent-transcripts`) — already written by the environment; you can **locate** it without treating the model’s recap as the only source.
2. **Readable export** — a script or IDE command that turns jsonl into markdown for reading and quoting.
3. **Short recap and alignment** — who understood what, what to record in the repo or KB.

If the context “axe” has already fallen, the **last full chunk** is still recoverable from a **file**, not from what remains visible in the chat after compression.

## Backup and boundaries

Common sense: **don’t** read-lock the live folder Cursor writes to — that breaks the workflow. Better **back up outward** (git, archive, another disk) and move **important** content into canon or notes instead of assuming the host will always preserve long history in a friendly form.

---

**Equal right to summarize** means the right to **initiate and run** explicit closure with a file-backed trail; **opaque compression** is not a substitute for that layer — it is a platform constraint to **route around** with export discipline, not to trust as memory.
