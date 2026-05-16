---
slug: agent-notes-mcp-2-one-config
title: "Agent Notes MCP 2.0: один конфиг для Cursor и IDE"
description: "Память агента перестала жить в разрозненных переменных окружения — один TOML для MCP, Cascade IDE и проверяемого hot-context."
date_display: "Май 2026"
order: 9
tags:
  - mcp
  - agents
  - knowledge-base
  - parity
---

Стек MCP уже умеет спрашивать компилятор, отладчик и тесты. Следующий слой — **что мы договорились помнить между сессиями**: playbooks, границы, маршрутизация контекста. Пока этот слой размазан по промптам, env и «магическим» путям, паритет ломается: Cursor видит одно, IDE — другое, человек правит третье.

**Agent Notes MCP 2.0** — ответ на эту рассинхронизацию: **один файл настроек** (TOML) как единственный источник правды для процесса MCP и для in-proc загрузки в [Cascade IDE](https://github.com/KarataevDmitry/cascade-ide).

## Что было неудобно

В ранних сборках путь к «канону» знаний часто тащили через переменные окружения и дубли в настройках хоста. Это работало в одной среде и ломалось в другой: поменял `mcp.json` — забыл `%LocalAppData%`, агент в IDE читает embedded-срез, а внешний MCP — другой корень `knowledge/`. Отладка превращалась в «а кто сейчас прав?», а не в проверку содержимого.

Для инженерии, где мы просим **одни и те же артефакты** у человека и модели, такая дрожь — лишний налог.

## Один TOML — SSOT

В 2.0 конфигурация описывается в **локальном TOML** (схема version 1): корни `knowledge/`, scope по workspace, опционально поверхность status на loopback.

- **Cursor / любой MCP-хост:** аргумент `--config` с путём к файлу (тот же файл, что ты правишь руками).
- **Cascade IDE:** в `settings.toml` секция `[agent_notes]` с `config_path` на **тот же** файл.

Никакого отдельного «канона в env» в supported path: загрузка через общую библиотеку [AIGuiders.AgentNotes.Core](https://github.com/KarataevDmitry/AIGuiders.AgentNotes.Core), один `Initialize`, один primary root.

Минимальный смысл файла:

```toml
version = 1

[knowledge]
primary = "personal"

[knowledge.roots]
personal = "/path/to/your/knowledge-repo"

[workspace]
default_scope = "mixed"
```

Путь к репозиторию знаний — твой; контракт — общий.

## Паритет инструментов: `knowledge_path`, не два имени

Публичные тулы MCP и команды IDE (`ide_read_knowledge_file`, `ide_memory_health`, …) говорят на одном языке аргументов: **`knowledge_path`** — корень с каталогом `knowledge/`. Старое имя `canon_path` в IDE ещё принимается как алиас, но новый текст и документация идут в одну сторону — чтобы не плодить два слоя «истины» в JSON.

Практический критерий: вызов `memory_health` в Cursor и `ide_memory_health` в Cascade IDE должен показывать **тот же** `notes_path` и тот же `resolved_scope`, если `config_path` указывает на один TOML.

## Наблюдаемость без облака

Отдельно в 2.0 появилась **localhost status surface** у процесса [agent-notes-mcp](https://github.com/KarataevDmitry/agent-notes-mcp): `/health`, HTML-дашборд, ring buffer последних вызовов тулов. Это не «аналитика в SaaS», а **короткий ответ на вопрос «жив ли сервер и что он только что делал»** — в духе того же паритета: факты на диске и в HTTP на loopback, а не ощущение из чата.

Cascade IDE эту HTTP-страницу не подменяет: IDE грузит Core **in-proc**. Зато на странице «готовность окружения» есть строка **agent-notes config (TOML)** — файл найден, primary root существует. В Dark Cockpit **норма — лампа не горит**: OK значит «тихо», а не «сломано».

## Зачем это в линии сайта

Репозитории на GitHub — доказательство. Тексты в разделе [/writing/](/writing/) — **зачем** устроена работа: общая опора, проверяемость, уважение к обоим участникам пары человек–агент. Эта заметка — про **инфраструктуру памяти**, без которой остальной MCP-стек остаётся умным, но амнестическим.

## Связанные тексты

- [**Зачем эти проекты и зачем паритет**](/ru/writing/why-these-projects-parity.html) — стек MCP и общая опора на факты.
- [**База знаний, доверие и любопытство**](/ru/writing/knowledge-base-trust-curiosity.html) — зачем вообще слой KB.
- [**Модель внимания Cascade IDE**](/ru/writing/cascade-ide-attention-cockpit.html) — кокпит и наблюдаемость агента в IDE.
- [**Равное право завершить и почему сжатие на хосте — слабый фундамент**](/ru/writing/summarization-parity-and-host-summary.html) — верифицируемые артефакты вместо непрозрачного summary.

Код и ADR в репозиториях: [agent-notes-mcp](https://github.com/KarataevDmitry/agent-notes-mcp), [cascade-ide](https://github.com/KarataevDmitry/cascade-ide) (ветка `develop`, ADR 0118).
