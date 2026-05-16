---
slug: dotnet-debug-mcp
title: "dotnet-debug-mcp"
description: "DAP-отладка из чата: launch/attach, шаги, брейкпоинты, стек и переменные."
subtitle: "DAP-отладка .NET для агентных сценариев"
repo_url: "https://github.com/AI-Guiders/dotnet-debug-mcp"
order: 2
---

dotnet-debug-mcp открывает воспроизводимую отладку через MCP: launch/attach, брейкпоинты, шаги, стек вызовов и просмотр переменных.

| Возможность | Зачем нужна | Статус |
|---|---|---|
| Launch / attach | Отладка с чистого старта или подключение к процессу | Production |
| Step / continue / stop | Детерминированное расследование поведения в рантайме | Production |
| Stack + scopes + variables | Опора на факты рантайма, а не на догадки | Production |
| Связка с resolve_breakpoint | Переход от символа к первой исполняемой строке | Integrated workflow |
