---
slug: dotnet-build-test-mcp
title: "dotnet-build-test-mcp"
description: "Структурированный build/test с очередью и компактной подачей ошибок для агентных сценариев."
subtitle: "Структурированный build/test для ассистентов"
repo_url: "https://github.com/AI-Guiders/dotnet-build-test-mcp"
order: 3
---

dotnet-build-test-mcp запускает `dotnet build` и `dotnet test` с очередью и нормализованным выводом, чтобы ассистент работал от сути ошибок.

| Возможность | Зачем нужна | Статус |
|---|---|---|
| Очередь build (single-flight) | Исключает конфликтующие параллельные прогоны | Production |
| Структурированные ошибки | Машиночитаемые диагнозы вместо стены логов | Production |
| Сводки тестов | Быстрый pass/fail и целевой контекст на следующий шаг | Production |
| Связка с Roslyn diagnostics | Сочетает статическую и runtime-проверку | Recommended workflow |
