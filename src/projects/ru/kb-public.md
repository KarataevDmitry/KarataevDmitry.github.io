---
slug: kb-public
title: "kb-public"
description: "Публичный read-only срез knowledge base для агентов: плейбуки, ядро целостности, маршрутизация доменов (CC BY-SA)."
subtitle: "Публичный срез KB для инструментов памяти агента"
repo_url: "https://github.com/AI-Guiders/kb-public"
order: 5
---

**kb-public** — опубликованная, только для чтения, выгрузка слоя знаний агента: плейбуки и справочные карточки в `knowledge/`, урезанный фрагмент hot-context, индексы маршрутизации — без закрытых деревьев `work/` и `personal/` полного канона.

| Что внутри | Зачем |
|---|---|
| `index-knowledge-router-v1.md` | Быстрая маршрутизация доменов при узком контексте |
| Ядро целостности (`META/integrity-core.md`, POST spec) | Необсуждаемый safety-базис для потребителей |
| Доменные playbook’и (`worlds/*`, HCI, Git, KE, …) | Операционные контракты, на которые может ссылаться агент |
| `SHOWCASE.md` | Онбординг без загрузки всего дерева |

Контент — **CC BY-SA 4.0** (см. `knowledge/README.md` и `PUBLISHING.md` в репозитории). MCP-сервер, который читает полный закрытый канон, — отдельно: **[agent-notes-mcp](https://github.com/AI-Guiders/agent-notes-mcp)** (код MIT).

**Связанные тексты:** [База знаний, доверие и любопытство](/ru/writing/knowledge-base-trust-curiosity.html) · [Agent Notes MCP 2.0: один конфиг](/ru/writing/agent-notes-mcp-2-one-config.html)
