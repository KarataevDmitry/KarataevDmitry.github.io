---
slug: roslyn-mcp
title: "RoslynMcp"
description: "MCP на Roslyn: диагностики, символы и безопасные рефакторинги для агента."
subtitle: "MCP поверх Roslyn для семантических операций C#"
repo_url: "https://github.com/KarataevDmitry/RoslynMcp"
order: 1
---

RoslynMcp даёт ассистенту IDE-уровень понимания кода: символы, диагностики, code actions, навигацию и управляемые рефакторинги.

| Возможность | Зачем нужна | Статус |
|---|---|---|
| Диагностики + code actions | Исправление компиляции и анализаторов по точным позициям | Production |
| Find usages / rename | Безопасные сквозные рефакторинги вместо текстовых замен | Production |
| Workspace navigation context | Граф связанных файлов для планирования правок | New |
| Resolve breakpoint by symbol | Связка семантики кода и точки входа в отладку | New |

**Связанный текст:** [Roslyn MCP: навигация по solution и что это даёт агенту](/ru/writing/roslyn-mcp-workspace-navigation-for-agents.html)
