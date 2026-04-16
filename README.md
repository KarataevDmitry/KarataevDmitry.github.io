# KarataevDmitry.github.io

GitHub Pages site: **Settings → Pages** uses the **`docs/`** folder on **`main`**. The live site root is the contents of `docs/` — push to `main` to update https://karataevdmitry.github.io/ .

## Writing (Markdown → HTML)

- **Source:** `src/writing/en/*.md` and `src/writing/ru/*.md` — YAML front matter (`slug`, `title`, `description`, `date_display`, `order`) + Markdown body.
- **Output (generated, commit after edit):** `docs/writing/` and `docs/ru/writing/` — `index.html` plus one `.html` per slug.
- **Layout CSS:** `docs/assets/css/articles.css` (hand-edited; not overwritten by the script).

**Requirements:** [.NET SDK](https://dotnet.microsoft.com/download) and [dotnet-script](https://github.com/dotnet-script/dotnet-script) (`dotnet tool install -g dotnet-script`).

From the repository root:

```bash
dotnet script tools/build-writing.csx
```

Optional: pass the repo root explicitly if the current directory is not the site repo:

```bash
dotnet script tools/build-writing.csx -- "D:\path\to\KarataevDmitry.github.io"
```

**New article:** add `my-slug.md` under both `src/writing/en/` and `src/writing/ru/` with the same `slug`, run the script, commit `src/` and `docs/`.

## Landing regression guard

Главные страницы `docs/index.html` и `docs/ru/index.html` правятся вручную; при массовой заливке лендинга легко потерять пункты **Writing** / **Тексты** и ссылки «зачем проекты» в блоке проектов.

После правок лендинга (или перед коммитом):

```bash
pwsh -NoProfile -File tools/assert-landing-invariants.ps1
```

На **push/PR в `main`** тот же скрипт запускается в GitHub Actions (workflow `.github/workflows/landing-invariants.yml`).
