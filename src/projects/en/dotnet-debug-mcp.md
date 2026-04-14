---
slug: dotnet-debug-mcp
title: "dotnet-debug-mcp"
description: "DAP debugging from chat: launch/attach, stepping, breakpoints, stacks, and variables."
subtitle: "DAP-based .NET debugging for agents"
repo_url: "https://github.com/KarataevDmitry/dotnet-debug-mcp"
order: 2
---

dotnet-debug-mcp exposes reproducible debugger control over MCP: launch/attach, breakpoints, stepping, stack frames, and variable inspection.

| Capability | Why it matters | Status |
|---|---|---|
| Launch / attach | Debug from clean start or running process in the same flow | Production |
| Step / continue / stop | Deterministic runtime investigation in chat-driven workflows | Production |
| Stack + scopes + variables | Grounds analysis in runtime facts, not inferred behavior | Production |
| Roslyn breakpoint resolver pairing | Semantic symbol selection to runtime stop line | Integrated workflow |
