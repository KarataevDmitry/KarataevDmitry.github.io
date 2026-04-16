# Provenance / соавторство (опционально)

Сборка `tools/build-writing.csx` умеет вывести **компактный блок раскрытия** под заголовком статьи (между `<header>` и телом), если в YAML включён флаг и заполнены поля.

## По умолчанию

- **`show_provenance` не указан или `false`** — блок **не** попадает в HTML. Существующие статьи менять не нужно.
- **`show_provenance: true`**, но **ни одно** из полей ниже не заполнено — блок **не** рендерится (чтобы не показывать пустой раскрывающийся блок).

## Поля YAML (snake_case)

| Ключ | Тип | Описание |
|------|-----|----------|
| `show_provenance` | bool | Включить блок только при `true`. |
| `provenance_author` | string | Человек-автор (как хочешь подписать на сайте). |
| `provenance_coauthors` | список строк | Инструменты / модели / агенты (имена строками). |
| `provenance_contribution` | string | Кто что сделал (роли, вклад). Многострочный текст сохраняет переносы. |
| `provenance_human_final` | string | Финальная правка, ответственность, согласование с человеком. |

Язык подписей в HTML (`Author` / `Автор` и т.д.) задаётся **папкой** исходника: `src/writing/en/` — английские, `src/writing/ru/` — русские.

## Пример

```yaml
---
slug: example
title: Example
description: Short blurb
date_display: April 2026
order: 10
show_provenance: true
provenance_author: Dmitry Karataev
provenance_coauthors:
  - Example model
provenance_contribution: |
  Draft outline and phrasing suggestions from the tool;
  structure and edits by the author.
provenance_human_final: Final wording and publication decision by the author.
---

Body markdown here.
```

## Политика

Пока **не** добавляй `show_provenance: true` в уже опубликованные тексты без явного решения «показывать слой раскрытия на сайте». Подготовка в репозитории — это только **возможность** включить блок позже.

Канон рассуждений и уровней раскрытия в личной KB: `personal/comet-kdgio-coauthorship-disclosure-note-v1.md` (репозиторий agent-notes).
