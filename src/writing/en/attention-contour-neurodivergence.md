---
slug: attention-contour-neurodivergence
title: "Attention, friction, and neurodivergence in the IDE"
description: "A design hypothesis: a stable cockpit-style attention hierarchy can reduce unnecessary friction for many developers—including challenges often reported by neurodivergent engineers. Not medical advice."
date_display: "April 2026"
order: 6
---

This note connects three themes: [**Cascade IDE’s cockpit-inspired attention model**](/writing/cascade-ide-attention-cockpit.html),
the idea of **friction from the environment** (not “bad people”), and **neurodivergence**
—for example **ADHD**, which I mention because it is common in engineering
samples and in public surveys.

It is **not** a claim that an interface treats a diagnosis. It **is** an argument
that **how attention is structured in software** changes the cost of staying
oriented—and that cost matters for many brains, including neurodivergent ones.

## What “attention contour” is trying to buy

The cockpit metaphor in **ADR 0021** is about **roles**, not wallpaper: a stable
**forward / editor** anchor, **secondary** surfaces you open on purpose (MFD-shaped),
and a **consolidated alerting** channel (EICAS-shaped)—instead of every tool
fighting for the same z-order.

That targets a familiar pain in knowledge work: **context switches**, **competing
demands**, **banner blindness**, and the slow rebuild of “what mattered last.”
Those pains show up in qualitative work on **software engineers with ADHD** and
in large **self-report** surveys (e.g. Stack Overflow Developer Survey items on
concentration/memory—often cited in academic papers at roughly **~10%** of
respondents; compare to general-population ADHD prevalence estimates, which are
lower and methodology-sensitive).

Nothing here replaces a clinician. The point is narrower: **if** a profession
clusters people who struggle with sustained attention under interruption-heavy
UIs, **then** an IDE that externalizes priority and state is not a niche luxury.

## Friction-first, environment-first

The same line of thought appears in work on **friction** in products and
processes: systems fail when the **environment** makes the wrong behaviour
easier than the right one. Reducing **spurious** friction—predictable places for
truth, fewer surprise surfaces—is aligned with that stance. See also
[Knowledge base, trust, and curiosity](/writing/knowledge-base-trust-curiosity.html)
on **provisional trust** and **inspectable** ground between human and agent.

## Where this may overlap with ADHD / neurodivergence (carefully)

Executive-function challenges often include **restarting after interruption**,
**prioritizing under noise**, and **holding several channels in mind**. A UI that
**separates** primary work from secondary tools and **centralizes** alerts does
not “fix” ADHD—but it can **lower the tax** on the same skills that interruption
taxes for everyone, sometimes asymmetrically.

**Parity** with the toolchain ([Why these projects, and why parity matters](/writing/why-these-projects-parity.html))
helps too: less re-derivation of reality from chat prose, more **shared artefacts**
(tests, diagnostics)—another form of **externalized** state.

## What this is not

- Not a substitute for accommodations, coaching, or medical care.
- Not a promise that one layout fits all: neurodivergence is heterogeneous;
  some people need **flexibility** and **density controls**, not only structure.
- Not finished product evidence: Cascade is **in development**; this is a
  **design rationale** bridge for readers.

## Related

- [**Cascade IDE’s cockpit-inspired attention model**](/writing/cascade-ide-attention-cockpit.html) — cockpit metaphor, PFD/MFD/EICAS
- **Trust, KB, curiosity:** [Knowledge base, trust, and curiosity](/writing/knowledge-base-trust-curiosity.html)
- **Tools and parity:** [Why these projects, and why parity matters](/writing/why-these-projects-parity.html)
- **Methodology:** [Why Agent-First Learn exists](/writing/why-agent-first-learn.html)
- **Agile in spirit:** [Why this human–agent workspace is Agile in spirit](/writing/agent-workspace-agile.html)
