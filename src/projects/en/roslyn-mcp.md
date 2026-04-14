---
slug: roslyn-mcp
title: "RoslynMcp"
description: "MCP over Roslyn: diagnostics, symbol intelligence, and safe refactoring flows for agents."
subtitle: "MCP over Roslyn for C# semantic operations"
repo_url: "https://github.com/KarataevDmitry/RoslynMcp"
order: 1
---

RoslynMcp gives an assistant IDE-grade semantic context: symbols, diagnostics, code actions, navigation, and controlled refactoring tools.

| Capability | Why it matters | Status |
|---|---|---|
| Diagnostics + code actions | Fix compile and analyzer issues from structured positions | Production |
| Find usages / rename | Safe cross-solution refactors instead of text replacements | Production |
| [Workspace navigation context](/writing/roslyn-mcp-workspace-navigation-for-agents.html) | Related-file graph for planning edits in partial/test-heavy repos | New |
| Breakpoint resolution by symbol | Bridges semantic code selection and debugger entry points | New |


