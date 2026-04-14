---
slug: dotnet-build-test-mcp
title: "dotnet-build-test-mcp"
description: "Structured build/test output with queueing and compact failures for agent workflows."
subtitle: "Structured build/test execution for assistants"
repo_url: "https://github.com/KarataevDmitry/dotnet-build-test-mcp"
order: 3
---

dotnet-build-test-mcp executes `dotnet build` and `dotnet test` with queueing and normalized output, so assistants can reason from concise failures.

| Capability | Why it matters | Status |
|---|---|---|
| Build queue (single-flight) | Prevents overlapping runs and noisy contention | Production |
| Structured error extraction | Gives agents machine-usable diagnostics quickly | Production |
| Test run summaries | Fast pass/fail + targeted context for next iteration | Production |
| Pairing with Roslyn diagnostics | Combines static + runtime verification loops | Recommended workflow |
