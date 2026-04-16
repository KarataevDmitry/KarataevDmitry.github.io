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

## Готовые формулировки (RU / EN)

Ниже — тексты, которые можно **почти без правок** перенести в YAML или в сопроводительный абзац. Они стыкуются с линией сайта про инфраструктуру для агентов, agent-first collaboration и общее knowledge-space. Варианты предложены в обсуждении с **Comet** (Perplexity); при желании укажи источник у себя в KB, на странице он не обязателен.

### Короткий вариант (карточка / одна мысль)

**RU.** Этот материал подготовлен Дмитрием Каратаевым в сотрудничестве с агентами. Участие агентов может включать обсуждение структуры, критику черновика, языковую полировку и проверку формулировок; финальная редактура и ответственность за текст остаются за человеком.

**EN.** This piece was created by Dmitry Karataev in collaboration with agents. Agents may contribute to outlining, draft critique, language refinement, and phrasing review; final editing and responsibility for the text remain with the human author.

*В YAML:* весь абзац в `provenance_contribution` или первое предложение в `provenance_contribution`, второе — в `provenance_human_final` (как удобнее для смысла).

### Тёплый вариант (паритет, «по-человечески»)

**RU.** Этот текст написан человеком не в одиночку, а в сотрудничестве с агентами. Я рассматриваю такое соавторство не как скрытую автоматизацию, а как прозрачную совместную работу, где у каждого участника есть своя роль, а финальная ответственность за смысл и публикацию остаётся за мной.

**EN.** This text was not written by a human alone, but in collaboration with agents. I see this kind of co-authorship not as hidden automation, but as transparent joint work where each participant has a role, while final responsibility for meaning and publication remains with me.

*Совет по применению:* хорошо заходит как основной текст для `provenance_contribution` (два абзаца подряд в одном поле); при необходимости второй абзац можно вынести в `provenance_human_final`.

### Паспорт статьи (структурировано под поля блока)

| Смысл | RU | EN |
|--------|----|----|
| Автор | Дмитрий Каратаев | Dmitry Karataev |
| Соавторство | агенты / языковые модели | agents / language models |
| Вклад агентов | структура, обсуждение идей, критика черновика, языковая полировка | structure, idea exploration, draft critique, language refinement |
| Финальная редактура | человек | human (final editing) |
| Ответственность за публикацию | человек | human (publication responsibility) |

**RU — строки для копипаста**

- Автор: Дмитрий Каратаев
- Соавторство: агенты / языковые модели
- Возможный вклад агентов: структура, обсуждение идей, критика черновика, языковая полировка
- Финальная редактура: человек
- Ответственность за публикацию: человек

**EN — строки для копипаста**

- Author: Dmitry Karataev
- Co-authorship: agents / language models
- Possible agent contribution: structure, idea exploration, draft critique, language refinement
- Final editing: human
- Publication responsibility: human

*Маппинг в YAML:* `provenance_author` = имя; `provenance_coauthors` = одна строка `агенты / языковые модели` (или две строки списка — на твой вкус); `provenance_contribution` = строка про вклад; `provenance_human_final` = две короткие строки про финальную редактуру и публикацию, например:

```yaml
provenance_human_final: |
  Финальная редактура: человек.
  Ответственность за публикацию: человек.
```

(EN: `Final editing: human.` / `Publication responsibility: human.`)

### Когда что брать

- **Короткий** — если нужна плотная формула без лишних слов.
- **Тёплый** — для обычных текстов, где важно звучание доверия и паритета.
- **Паспорт** — для заметных статей или проектных страниц, где полезна явная структура «кто / что / финал».

## Политика

Пока **не** добавляй `show_provenance: true` в уже опубликованные тексты без явного решения «показывать слой раскрытия на сайте». Подготовка в репозитории — это только **возможность** включить блок позже.

Канон рассуждений и уровней раскрытия в личной KB: `personal/comet-kdgio-coauthorship-disclosure-note-v1.md` (репозиторий agent-notes).
