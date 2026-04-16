#!/usr/bin/env dotnet-script
#r "nuget: Markdig, 0.37.0"
#r "nuget: YamlDotNet, 16.3.0"

#nullable enable

using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var repoRoot = Args.Count > 0 && !string.IsNullOrWhiteSpace(Args[0])
    ? Path.GetFullPath(Args[0])
    : Directory.GetCurrentDirectory();

var srcEn = Path.Combine(repoRoot, "src", "writing", "en");
var srcRu = Path.Combine(repoRoot, "src", "writing", "ru");
var docsOut = Path.Combine(repoRoot, "docs");
var writingEnOut = Path.Combine(docsOut, "writing");
var writingRuOut = Path.Combine(docsOut, "ru", "writing");

if (!Directory.Exists(srcEn) || !Directory.Exists(srcRu))
{
    Console.Error.WriteLine("Expected src/writing/en and src/writing/ru under: " + repoRoot);
    Environment.Exit(1);
}

Directory.CreateDirectory(writingEnOut);
Directory.CreateDirectory(writingRuOut);

var mdPipeline = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .Build();

var yaml = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .IgnoreUnmatchedProperties()
    .Build();

var enArticles = LoadArticles(srcEn, yaml, mdPipeline);
var ruArticles = LoadArticles(srcRu, yaml, mdPipeline);

foreach (var a in enArticles)
    WriteArticlePage(a, "en", writingEnOut, "/writing/");
foreach (var a in ruArticles)
    WriteArticlePage(a, "ru", writingRuOut, "/ru/writing/");

WriteIndex(enArticles.OrderBy(x => x.Order).ThenBy(x => x.Slug).ToList(), "en", writingEnOut);
WriteIndex(ruArticles.OrderBy(x => x.Order).ThenBy(x => x.Slug).ToList(), "ru", writingRuOut);

WriteTagDirectory(enArticles, "en", writingEnOut, "/writing/");
WriteTagDirectory(ruArticles, "ru", writingRuOut, "/ru/writing/");

WriteAtomFeed(enArticles, "en", writingEnOut, "/writing/");
WriteAtomFeed(ruArticles, "ru", writingRuOut, "/ru/writing/");

Console.WriteLine($"Writing: {enArticles.Count} EN + {ruArticles.Count} RU articles → docs/writing/ (+ tags, atom)");

static List<Article> LoadArticles(string dir, IDeserializer yaml, MarkdownPipeline mdPipeline)
{
    var list = new List<Article>();
    foreach (var path in Directory.EnumerateFiles(dir, "*.md"))
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var (front, body) = SplitFrontMatter(text);
        var meta = yaml.Deserialize<FrontMatter>(front) ?? throw new InvalidOperationException("YAML: " + path);
        if (string.IsNullOrWhiteSpace(meta.Slug) || string.IsNullOrWhiteSpace(meta.Title))
            throw new InvalidOperationException("slug/title required: " + path);

        var html = Markdown.ToHtml(body.Trim(), mdPipeline);
        var tags = NormalizeTags(meta.Tags ?? new List<string>());
        var provenance = BuildProvenanceFromFrontMatter(meta);
        list.Add(new Article(meta.Slug, meta.Title, meta.Description ?? "", meta.Date_display ?? "", meta.Order, html, tags, provenance));
    }
    return list;
}

static CoauthorshipProvenance? BuildProvenanceFromFrontMatter(FrontMatter meta)
{
    if (!meta.Show_provenance)
        return null;

    var author = TrimOrNull(meta.Provenance_author);
    var coauthors = meta.Provenance_coauthors?
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(s => s.Trim())
        .ToList() ?? new List<string>();
    var contribution = TrimOrNull(meta.Provenance_contribution);
    var humanFinal = TrimOrNull(meta.Provenance_human_final);

    if (author is null && coauthors.Count == 0 && contribution is null && humanFinal is null)
        return null;

    return new CoauthorshipProvenance(author, coauthors, contribution, humanFinal);
}

static string? TrimOrNull(string? s)
{
    if (string.IsNullOrWhiteSpace(s))
        return null;
    var t = s.Trim();
    return t.Length == 0 ? null : t;
}

/// <summary>Уникальные теги: slug для URL, display — первая встреченная строка из YAML.</summary>
static IReadOnlyList<ArticleTag> NormalizeTags(List<string> raw)
{
    if (raw.Count == 0)
        return Array.Empty<ArticleTag>();

    var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var t in raw)
    {
        if (string.IsNullOrWhiteSpace(t))
            continue;
        var s = t.Trim();
        string display;
        string slug;
        var colon = s.IndexOf(':');
        if (colon > 0 && colon < s.Length - 1)
        {
            display = s[..colon].Trim();
            var slugRaw = s[(colon + 1)..].Trim();
            slug = TagToSlug(slugRaw);
            if (string.IsNullOrEmpty(slug))
                slug = TagToSlug(display);
            if (string.IsNullOrEmpty(display))
                display = string.IsNullOrEmpty(slugRaw) ? slug : slugRaw;
        }
        else
        {
            display = s;
            slug = TagToSlug(s);
        }

        if (string.IsNullOrEmpty(slug))
            continue;
        if (!map.ContainsKey(slug))
            map[slug] = display;
    }

    return map.OrderBy(kv => kv.Key, StringComparer.Ordinal)
        .Select(kv => new ArticleTag(kv.Value, kv.Key))
        .ToList();
}

static string TagToSlug(string s)
{
    var lower = s.Trim().ToLowerInvariant();
    var sb = new StringBuilder();
    foreach (var c in lower)
    {
        if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
            sb.Append(c);
        else if (c is ' ' or '-' or '_')
        {
            if (sb.Length > 0 && sb[^1] != '-')
                sb.Append('-');
        }
    }

    var r = sb.ToString().Trim('-');
    r = Regex.Replace(r, "-{2,}", "-");
    return r;
}

static (string Yaml, string Body) SplitFrontMatter(string text)
{
    if (!text.StartsWith("---", StringComparison.Ordinal))
        return ("", text);
    var end = text.IndexOf("\n---", 3, StringComparison.Ordinal);
    if (end < 0)
        return ("", text);
    var yamlBlock = text[3..end].Trim();
    var body = text[(end + "\n---".Length)..];
    return (yamlBlock, body);
}

static void WriteArticlePage(Article a, string lang, string outDir, string writingBase)
{
    var isEn = lang == "en";
    var labels = isEn
        ? new NavLabels("Skip to content", "Main navigation", "Language", "About", "Projects", "Writing", "Experience", "Docs", "All writing", "Email", "Telegram", "GitHub", "All tags")
        : new NavLabels("К содержанию", "Основная навигация", "Язык", "О себе", "Проекты", "Тексты", "Опыт", "Документы", "Все тексты", "Email", "Telegram", "GitHub", "Все теги");

    var home = isEn ? "/" : "/ru/";
    var fileName = a.Slug + ".html";
    var enUrl = "/writing/" + fileName;
    var ruUrl = "/ru/writing/" + fileName;
    var pageTitle = WebUtility.HtmlEncode(a.Title) + (isEn ? " — Dmitry Karataev" : " — Дмитрий Каратаев");
    var siteName = isEn ? "Dmitry Karataev" : "Дмитрий Каратаев";
    var logo = isEn ? "D<span>.</span>Karataev" : "Д<span>.</span>Каратаев";
    var tagsBase = writingBase + "tag/";

    var sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine($"<html lang=\"{lang}\">");
    sb.AppendLine("<head>");
    sb.AppendLine("  <meta charset=\"utf-8\" />");
    sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
    sb.AppendLine($"  <title>{pageTitle}</title>");
    sb.AppendLine($"  <meta name=\"description\" content=\"{WebUtility.HtmlEncode(a.Description)}\" />");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"en\" href=\"{enUrl}\" />");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"ru\" href=\"{ruUrl}\" />");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"x-default\" href=\"{enUrl}\" />");
    sb.AppendLine("  <link rel=\"icon\" href=\"data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>&#x1F4BB;</text></svg>\" />");
    sb.AppendLine("  <link rel=\"stylesheet\" href=\"/assets/css/articles.css\" />");
    sb.AppendLine($"  <link rel=\"alternate\" type=\"application/atom+xml\" title=\"{(isEn ? "Writing (Atom)" : "Тексты (Atom)")}\" href=\"{(isEn ? "/writing/atom.xml" : "/ru/writing/atom.xml")}\" />");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");
    sb.AppendLine($"  <a href=\"#main\" class=\"skip-link\">{labels.SkipToContent}</a>");
    sb.AppendLine();
    sb.AppendLine($"  <nav aria-label=\"{WebUtility.HtmlEncode(labels.NavMain)}\">");
    sb.AppendLine("    <div class=\"nav-inner\">");
    sb.AppendLine($"      <div class=\"logo\"><a href=\"{home}\">{logo}</a></div>");
    sb.AppendLine("      <div class=\"nav-right\">");
    sb.AppendLine("        <ul>");
    sb.AppendLine($"          <li><a href=\"{home}#about\">{labels.About}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#projects\">{labels.Projects}</a></li>");
    sb.AppendLine($"          <li><a href=\"{writingBase}\" aria-current=\"page\">{labels.Writing}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#experience\">{labels.Experience}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#documents\">{labels.Docs}</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine($"        <ul class=\"nav-lang\" aria-label=\"{WebUtility.HtmlEncode(labels.Language)}\">");
    sb.AppendLine($"          <li><a href=\"{enUrl}\" lang=\"en\"{(isEn ? " aria-current=\"page\"" : "")}>EN</a></li>");
    sb.AppendLine("          <li class=\"nav-sep\" aria-hidden=\"true\">|</li>");
    sb.AppendLine($"          <li><a href=\"{ruUrl}\" lang=\"ru\"{(!isEn ? " aria-current=\"page\"" : "")}>RU</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine("      </div>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </nav>");
    sb.AppendLine();
    sb.AppendLine("  <main id=\"main\" class=\"container\">");
    sb.AppendLine("    <article class=\"article-wrap\">");
    sb.AppendLine($"      <p class=\"article-back\"><a href=\"{writingBase}\">&larr; {labels.AllWriting}</a> · <a href=\"{writingBase}tags.html\">{labels.AllTags}</a> · <a href=\"{writingBase}atom.xml\">{(isEn ? "Atom feed" : "Лента Atom")}</a></p>");
    sb.AppendLine();
    sb.AppendLine("      <header class=\"article-header\">");
    sb.AppendLine($"        <h1>{WebUtility.HtmlEncode(a.Title)}</h1>");
    sb.AppendLine($"        <p class=\"article-meta\">{WebUtility.HtmlEncode(a.DateDisplay)}</p>");
    if (a.Tags.Count > 0)
    {
        sb.AppendLine("        <p class=\"article-tags\" aria-label=\"tags\">");
        foreach (var t in a.Tags)
            sb.AppendLine($"          <a class=\"tag-pill\" href=\"{tagsBase}{t.Slug}.html\">{WebUtility.HtmlEncode(t.Display)}</a>");
        sb.AppendLine("        </p>");
    }
    sb.AppendLine("      </header>");
    if (a.Provenance is { } pv)
    {
        sb.AppendLine();
        sb.AppendLine(Indent(BuildProvenanceAside(pv, isEn), "      "));
        sb.AppendLine();
    }
    else
    {
        sb.AppendLine();
    }
    sb.AppendLine("      <div class=\"prose\">");
    sb.AppendLine(Indent(a.BodyHtml, "        "));
    sb.AppendLine("      </div>");
    sb.AppendLine("    </article>");
    sb.AppendLine("  </main>");
    sb.AppendLine();
    sb.AppendLine("  <footer id=\"contact\">");
    sb.AppendLine("    <div class=\"container\">");
    sb.AppendLine("      <div class=\"contact-links\">");
    sb.AppendLine($"        <a href=\"mailto:dkarataev1990@gmail.com\">{labels.Email}</a>");
    sb.AppendLine($"        <a href=\"https://t.me/Krawler\">{labels.Telegram}</a>");
    sb.AppendLine($"        <a href=\"https://github.com/KarataevDmitry\">{labels.GitHub}</a>");
    sb.AppendLine("      </div>");
    sb.AppendLine($"      <p>{siteName} &copy; 2026.</p>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </footer>");
    sb.AppendLine("</body>");
    sb.AppendLine("</html>");

    File.WriteAllText(Path.Combine(outDir, fileName), sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static void WriteIndex(List<Article> articles, string lang, string outDir)
{
    var isEn = lang == "en";
    var labels = isEn
        ? new NavLabels("Skip to content", "Main navigation", "Language", "About", "Projects", "Writing", "Experience", "Docs", "All writing", "Email", "Telegram", "GitHub", "All tags")
        : new NavLabels("К содержанию", "Основная навигация", "Язык", "О себе", "Проекты", "Тексты", "Опыт", "Документы", "Все тексты", "Email", "Telegram", "GitHub", "Все теги");

    var home = isEn ? "/" : "/ru/";
    var writingBase = isEn ? "/writing/" : "/ru/writing/";
    var pageTitle = isEn ? "Writing — Dmitry Karataev" : "Тексты — Дмитрий Каратаев";
    var pageDesc = isEn
        ? "Essays on MCP tooling, agent infrastructure, and human–agent parity."
        : "Заметки про MCP, инфраструктуру агентов и паритет «люди — агенты».";
    var intro = isEn
        ? "Short pieces on why the open-source MCP stack exists and what &ldquo;parity&rdquo; between people and agents is meant to achieve."
        : "Короткие заметки о том, зачем собран открытый стек MCP и что имеется в виду под паритетом людей и агентов.";
    var h1 = isEn ? "Writing" : "Тексты";
    var back = isEn ? "Home" : "На главную";
    var logo = isEn ? "D<span>.</span>Karataev" : "Д<span>.</span>Каратаев";
    var siteName = isEn ? "Dmitry Karataev" : "Дмитрий Каратаев";
    var tagsBase = writingBase + "tag/";

    var tagCounts = new Dictionary<string, (string Display, int Count)>(StringComparer.OrdinalIgnoreCase);
    foreach (var a in articles)
    {
        foreach (var t in a.Tags)
        {
            if (!tagCounts.TryGetValue(t.Slug, out var cur))
                tagCounts[t.Slug] = (t.Display, 1);
            else
                tagCounts[t.Slug] = (cur.Display, cur.Count + 1);
        }
    }

    var sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine($"<html lang=\"{lang}\">");
    sb.AppendLine("<head>");
    sb.AppendLine("  <meta charset=\"utf-8\" />");
    sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
    sb.AppendLine($"  <title>{WebUtility.HtmlEncode(pageTitle)}</title>");
    sb.AppendLine($"  <meta name=\"description\" content=\"{WebUtility.HtmlEncode(pageDesc)}\" />");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"en\" href=\"/writing/\" />");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"ru\" href=\"/ru/writing/\" />");
    sb.AppendLine("  <link rel=\"alternate\" hreflang=\"x-default\" href=\"/writing/\" />");
    sb.AppendLine("  <link rel=\"icon\" href=\"data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>&#x1F4BB;</text></svg>\" />");
    sb.AppendLine("  <link rel=\"stylesheet\" href=\"/assets/css/articles.css\" />");
    sb.AppendLine($"  <link rel=\"alternate\" type=\"application/atom+xml\" title=\"{(isEn ? "Writing (Atom)" : "Тексты (Atom)")}\" href=\"{(isEn ? "/writing/atom.xml" : "/ru/writing/atom.xml")}\" />");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");
    sb.AppendLine($"  <a href=\"#main\" class=\"skip-link\">{labels.SkipToContent}</a>");
    sb.AppendLine();
    sb.AppendLine($"  <nav aria-label=\"{WebUtility.HtmlEncode(labels.NavMain)}\">");
    sb.AppendLine("    <div class=\"nav-inner\">");
    sb.AppendLine($"      <div class=\"logo\"><a href=\"{home}\">{logo}</a></div>");
    sb.AppendLine("      <div class=\"nav-right\">");
    sb.AppendLine("        <ul>");
    sb.AppendLine($"          <li><a href=\"{home}#about\">{labels.About}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#projects\">{labels.Projects}</a></li>");
    sb.AppendLine($"          <li><a href=\"{writingBase}\" aria-current=\"page\">{labels.Writing}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#experience\">{labels.Experience}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#documents\">{labels.Docs}</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine($"        <ul class=\"nav-lang\" aria-label=\"{WebUtility.HtmlEncode(labels.Language)}\">");
    sb.AppendLine($"          <li><a href=\"/writing/\" lang=\"en\"{(isEn ? " aria-current=\"page\"" : "")}>EN</a></li>");
    sb.AppendLine("          <li class=\"nav-sep\" aria-hidden=\"true\">|</li>");
    sb.AppendLine($"          <li><a href=\"/ru/writing/\" lang=\"ru\"{(!isEn ? " aria-current=\"page\"" : "")}>RU</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine("      </div>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </nav>");
    sb.AppendLine();
    sb.AppendLine("  <main id=\"main\" class=\"container\">");
    sb.AppendLine("    <div class=\"article-wrap\">");
    sb.AppendLine($"      <p class=\"article-back\"><a href=\"{home}\">&larr; {back}</a></p>");
    sb.AppendLine($"      <h1 class=\"page-title\">{WebUtility.HtmlEncode(h1)}</h1>");
    sb.AppendLine($"      <p class=\"page-intro\">{intro}</p>");
    sb.AppendLine($"      <p class=\"tags-browse\"><a href=\"{writingBase}tags.html\">{labels.AllTags}</a> · <a href=\"{writingBase}atom.xml\">{(isEn ? "Atom feed" : "Лента Atom")}</a></p>");
    if (tagCounts.Count > 0)
    {
        sb.AppendLine("      <ul class=\"tag-cloud\" aria-label=\"tags\">");
        foreach (var kv in tagCounts.OrderByDescending(x => x.Value.Count).ThenBy(x => x.Key))
        {
            sb.AppendLine("        <li>");
            sb.AppendLine($"          <a class=\"tag-pill\" href=\"{tagsBase}{kv.Key}.html\">{WebUtility.HtmlEncode(kv.Value.Display)}</a>");
            sb.AppendLine($"          <span class=\"tag-count\" aria-hidden=\"true\">{kv.Value.Count}</span>");
            sb.AppendLine("        </li>");
        }
        sb.AppendLine("      </ul>");
    }
    sb.AppendLine();
    sb.AppendLine("      <ul class=\"writing-list\">");
    foreach (var a in articles)
    {
        var href = writingBase.TrimEnd('/') + "/" + a.Slug + ".html";
        sb.AppendLine("        <li>");
        sb.AppendLine($"          <a class=\"title\" href=\"{href}\">{WebUtility.HtmlEncode(a.Title)}</a>");
        if (a.Tags.Count > 0)
        {
            sb.AppendLine("          <p class=\"writing-item-tags\">");
            foreach (var t in a.Tags)
                sb.AppendLine($"            <a class=\"tag-pill tag-pill--sm\" href=\"{tagsBase}{t.Slug}.html\">{WebUtility.HtmlEncode(t.Display)}</a>");
            sb.AppendLine("          </p>");
        }
        sb.AppendLine($"          <p class=\"blurb\">{WebUtility.HtmlEncode(a.Description)}</p>");
        sb.AppendLine("        </li>");
    }
    sb.AppendLine("      </ul>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </main>");
    sb.AppendLine();
    sb.AppendLine("  <footer id=\"contact\">");
    sb.AppendLine("    <div class=\"container\">");
    sb.AppendLine("      <div class=\"contact-links\">");
    sb.AppendLine($"        <a href=\"mailto:dkarataev1990@gmail.com\">{labels.Email}</a>");
    sb.AppendLine($"        <a href=\"https://t.me/Krawler\">{labels.Telegram}</a>");
    sb.AppendLine($"        <a href=\"https://github.com/KarataevDmitry\">{labels.GitHub}</a>");
    sb.AppendLine("      </div>");
    sb.AppendLine($"      <p>{siteName} &copy; 2026.</p>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </footer>");
    sb.AppendLine("</body>");
    sb.AppendLine("</html>");

    File.WriteAllText(Path.Combine(outDir, "index.html"), sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static void WriteTagDirectory(List<Article> articles, string lang, string outDir, string writingBase)
{
    var tagOut = Path.Combine(outDir, "tag");
    Directory.CreateDirectory(tagOut);

    var bySlug = new Dictionary<string, (string Display, List<Article> Items)>(StringComparer.OrdinalIgnoreCase);
    foreach (var a in articles)
    {
        foreach (var t in a.Tags)
        {
            if (!bySlug.TryGetValue(t.Slug, out var entry))
                bySlug[t.Slug] = (t.Display, new List<Article> { a });
            else if (!entry.Items.Any(x => x.Slug == a.Slug))
                entry.Items.Add(a);
        }
    }

    foreach (var kv in bySlug)
        WriteTagPage(kv.Key, kv.Value.Display, kv.Value.Items.OrderBy(x => x.Order).ThenBy(x => x.Slug).ToList(), lang, tagOut, writingBase);

    WriteTagsIndexPage(lang, outDir, writingBase, bySlug);
}

static void WriteTagsIndexPage(
    string lang,
    string outDir,
    string writingBase,
    Dictionary<string, (string Display, List<Article> Items)> bySlug)
{
    var isEn = lang == "en";
    var labels = isEn
        ? new NavLabels("Skip to content", "Main navigation", "Language", "About", "Projects", "Writing", "Experience", "Docs", "All writing", "Email", "Telegram", "GitHub", "All tags")
        : new NavLabels("К содержанию", "Основная навигация", "Язык", "О себе", "Проекты", "Тексты", "Опыт", "Документы", "Все тексты", "Email", "Telegram", "GitHub", "Все теги");

    var home = isEn ? "/" : "/ru/";
    var logo = isEn ? "D<span>.</span>Karataev" : "Д<span>.</span>Каратаев";
    var siteName = isEn ? "Dmitry Karataev" : "Дмитрий Каратаев";
    var pageTitle = isEn ? "Tags — Dmitry Karataev" : "Теги — Дмитрий Каратаев";
    var h1 = isEn ? "Tags" : "Теги";
    var back = isEn ? "Writing" : "Тексты";
    var intro = isEn
        ? "Browse pieces by topic. Tags use the same labels in EN and RU for each article pair."
        : "Тексты по темам. У пары EN/RU у статьи те же теги (латиницей в YAML).";

    var tagsBase = writingBase + "tag/";
    var sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine($"<html lang=\"{lang}\">");
    sb.AppendLine("<head>");
    sb.AppendLine("  <meta charset=\"utf-8\" />");
    sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
    sb.AppendLine($"  <title>{WebUtility.HtmlEncode(pageTitle)}</title>");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"en\" href=\"/writing/tags.html\" />");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"ru\" href=\"/ru/writing/tags.html\" />");
    sb.AppendLine("  <link rel=\"alternate\" hreflang=\"x-default\" href=\"/writing/tags.html\" />");
    sb.AppendLine("  <link rel=\"icon\" href=\"data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>&#x1F4BB;</text></svg>\" />");
    sb.AppendLine("  <link rel=\"stylesheet\" href=\"/assets/css/articles.css\" />");
    sb.AppendLine($"  <link rel=\"alternate\" type=\"application/atom+xml\" title=\"{(isEn ? "Writing (Atom)" : "Тексты (Atom)")}\" href=\"{(isEn ? "/writing/atom.xml" : "/ru/writing/atom.xml")}\" />");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");
    sb.AppendLine($"  <a href=\"#main\" class=\"skip-link\">{labels.SkipToContent}</a>");
    sb.AppendLine();
    sb.AppendLine($"  <nav aria-label=\"{WebUtility.HtmlEncode(labels.NavMain)}\">");
    sb.AppendLine("    <div class=\"nav-inner\">");
    sb.AppendLine($"      <div class=\"logo\"><a href=\"{home}\">{logo}</a></div>");
    sb.AppendLine("      <div class=\"nav-right\">");
    sb.AppendLine("        <ul>");
    sb.AppendLine($"          <li><a href=\"{home}#about\">{labels.About}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#projects\">{labels.Projects}</a></li>");
    sb.AppendLine($"          <li><a href=\"{writingBase}\" aria-current=\"page\">{labels.Writing}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#experience\">{labels.Experience}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#documents\">{labels.Docs}</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine($"        <ul class=\"nav-lang\" aria-label=\"{WebUtility.HtmlEncode(labels.Language)}\">");
    sb.AppendLine($"          <li><a href=\"/writing/tags.html\" lang=\"en\"{(isEn ? " aria-current=\"page\"" : "")}>EN</a></li>");
    sb.AppendLine("          <li class=\"nav-sep\" aria-hidden=\"true\">|</li>");
    sb.AppendLine($"          <li><a href=\"/ru/writing/tags.html\" lang=\"ru\"{(!isEn ? " aria-current=\"page\"" : "")}>RU</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine("      </div>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </nav>");
    sb.AppendLine();
    sb.AppendLine("  <main id=\"main\" class=\"container\">");
    sb.AppendLine("    <div class=\"article-wrap\">");
    sb.AppendLine($"      <p class=\"article-back\"><a href=\"{writingBase}\">&larr; {back}</a> · <a href=\"{writingBase}atom.xml\">{(isEn ? "Atom feed" : "Лента Atom")}</a></p>");
    sb.AppendLine($"      <h1 class=\"page-title\">{WebUtility.HtmlEncode(h1)}</h1>");
    sb.AppendLine($"      <p class=\"page-intro\">{intro}</p>");
    sb.AppendLine("      <ul class=\"tags-index-list\">");
    foreach (var kv in bySlug.OrderByDescending(x => x.Value.Items.Count).ThenBy(x => x.Key))
    {
        var n = kv.Value.Items.Count;
        sb.AppendLine("        <li>");
        sb.AppendLine($"          <a href=\"{tagsBase}{kv.Key}.html\">{WebUtility.HtmlEncode(kv.Value.Display)}</a>");
        sb.AppendLine($"          <span class=\"tags-index-count\">({n})</span>");
        sb.AppendLine("        </li>");
    }
    sb.AppendLine("      </ul>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </main>");
    sb.AppendLine();
    sb.AppendLine("  <footer id=\"contact\">");
    sb.AppendLine("    <div class=\"container\">");
    sb.AppendLine("      <div class=\"contact-links\">");
    sb.AppendLine($"        <a href=\"mailto:dkarataev1990@gmail.com\">{labels.Email}</a>");
    sb.AppendLine($"        <a href=\"https://t.me/Krawler\">{labels.Telegram}</a>");
    sb.AppendLine($"        <a href=\"https://github.com/KarataevDmitry\">{labels.GitHub}</a>");
    sb.AppendLine("      </div>");
    sb.AppendLine($"      <p>{siteName} &copy; 2026.</p>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </footer>");
    sb.AppendLine("</body>");
    sb.AppendLine("</html>");

    File.WriteAllText(Path.Combine(outDir, "tags.html"), sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static void WriteTagPage(string slug, string display, List<Article> items, string lang, string tagOutDir, string writingBase)
{
    var isEn = lang == "en";
    var labels = isEn
        ? new NavLabels("Skip to content", "Main navigation", "Language", "About", "Projects", "Writing", "Experience", "Docs", "All writing", "Email", "Telegram", "GitHub", "All tags")
        : new NavLabels("К содержанию", "Основная навигация", "Язык", "О себе", "Проекты", "Тексты", "Опыт", "Документы", "Все тексты", "Email", "Telegram", "GitHub", "Все теги");

    var home = isEn ? "/" : "/ru/";
    var logo = isEn ? "D<span>.</span>Karataev" : "Д<span>.</span>Каратаев";
    var siteName = isEn ? "Dmitry Karataev" : "Дмитрий Каратаев";
    var pageTitle = (isEn ? "Tag: " : "Тег: ") + display + (isEn ? " — Dmitry Karataev" : " — Дмитрий Каратаев");
    var h1 = (isEn ? "Tag: " : "Тег: ") + display;
    var back = isEn ? "Writing" : "Тексты";

    var sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine($"<html lang=\"{lang}\">");
    sb.AppendLine("<head>");
    sb.AppendLine("  <meta charset=\"utf-8\" />");
    sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
    sb.AppendLine($"  <title>{WebUtility.HtmlEncode(pageTitle)}</title>");
    sb.AppendLine("  <link rel=\"icon\" href=\"data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>&#x1F4BB;</text></svg>\" />");
    sb.AppendLine("  <link rel=\"stylesheet\" href=\"/assets/css/articles.css\" />");
    sb.AppendLine($"  <link rel=\"alternate\" type=\"application/atom+xml\" title=\"{(isEn ? "Writing (Atom)" : "Тексты (Atom)")}\" href=\"{(isEn ? "/writing/atom.xml" : "/ru/writing/atom.xml")}\" />");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");
    sb.AppendLine($"  <a href=\"#main\" class=\"skip-link\">{labels.SkipToContent}</a>");
    sb.AppendLine();
    sb.AppendLine($"  <nav aria-label=\"{WebUtility.HtmlEncode(labels.NavMain)}\">");
    sb.AppendLine("    <div class=\"nav-inner\">");
    sb.AppendLine($"      <div class=\"logo\"><a href=\"{home}\">{logo}</a></div>");
    sb.AppendLine("      <div class=\"nav-right\">");
    sb.AppendLine("        <ul>");
    sb.AppendLine($"          <li><a href=\"{home}#about\">{labels.About}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#projects\">{labels.Projects}</a></li>");
    sb.AppendLine($"          <li><a href=\"{writingBase}\" aria-current=\"page\">{labels.Writing}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#experience\">{labels.Experience}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#documents\">{labels.Docs}</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine($"        <ul class=\"nav-lang\" aria-label=\"{WebUtility.HtmlEncode(labels.Language)}\">");
    sb.AppendLine($"          <li><a href=\"/writing/tag/{slug}.html\" lang=\"en\"{(isEn ? " aria-current=\"page\"" : "")}>EN</a></li>");
    sb.AppendLine("          <li class=\"nav-sep\" aria-hidden=\"true\">|</li>");
    sb.AppendLine($"          <li><a href=\"/ru/writing/tag/{slug}.html\" lang=\"ru\"{(!isEn ? " aria-current=\"page\"" : "")}>RU</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine("      </div>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </nav>");
    sb.AppendLine();
    sb.AppendLine("  <main id=\"main\" class=\"container\">");
    sb.AppendLine("    <div class=\"article-wrap\">");
    sb.AppendLine($"      <p class=\"article-back\"><a href=\"{writingBase}\">&larr; {back}</a> · <a href=\"{writingBase}tags.html\">{labels.AllTags}</a> · <a href=\"{writingBase}atom.xml\">{(isEn ? "Atom feed" : "Лента Atom")}</a></p>");
    sb.AppendLine($"      <h1 class=\"page-title\">{WebUtility.HtmlEncode(h1)}</h1>");
    sb.AppendLine("      <ul class=\"writing-list\">");
    foreach (var a in items)
    {
        var href = writingBase.TrimEnd('/') + "/" + a.Slug + ".html";
        sb.AppendLine("        <li>");
        sb.AppendLine($"          <a class=\"title\" href=\"{href}\">{WebUtility.HtmlEncode(a.Title)}</a>");
        sb.AppendLine($"          <p class=\"blurb\">{WebUtility.HtmlEncode(a.Description)}</p>");
        sb.AppendLine("        </li>");
    }
    sb.AppendLine("      </ul>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </main>");
    sb.AppendLine();
    sb.AppendLine("  <footer id=\"contact\">");
    sb.AppendLine("    <div class=\"container\">");
    sb.AppendLine("      <div class=\"contact-links\">");
    sb.AppendLine($"        <a href=\"mailto:dkarataev1990@gmail.com\">{labels.Email}</a>");
    sb.AppendLine($"        <a href=\"https://t.me/Krawler\">{labels.Telegram}</a>");
    sb.AppendLine($"        <a href=\"https://github.com/KarataevDmitry\">{labels.GitHub}</a>");
    sb.AppendLine("      </div>");
    sb.AppendLine($"      <p>{siteName} &copy; 2026.</p>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </footer>");
    sb.AppendLine("</body>");
    sb.AppendLine("</html>");

    File.WriteAllText(Path.Combine(tagOutDir, slug + ".html"), sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static void WriteAtomFeed(List<Article> articles, string lang, string outDir, string writingBase)
{
    const string siteOrigin = "https://karataevdmitry.github.io";
    var isEn = lang == "en";
    var feedTitle = isEn ? "Writing — Dmitry Karataev" : "Тексты — Дмитрий Каратаев";
    var selfUrl = siteOrigin + (isEn ? "/writing/atom.xml" : "/ru/writing/atom.xml");
    var htmlBase = siteOrigin + writingBase.TrimEnd('/') + "/";
    var sorted = articles.OrderByDescending(x => x.Order).ThenBy(x => x.Slug).ToList();
    var feedUpdated = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);
    foreach (var a in sorted)
    {
        var u = AtomDate(a);
        if (u > feedUpdated)
            feedUpdated = u;
    }

    var sb = new StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    sb.AppendLine("<feed xmlns=\"http://www.w3.org/2005/Atom\">");
    sb.AppendLine($"  <title>{XmlText(feedTitle)}</title>");
    sb.AppendLine($"  <link href=\"{htmlBase}\" rel=\"alternate\" type=\"text/html\" />");
    sb.AppendLine($"  <link href=\"{selfUrl}\" rel=\"self\" type=\"application/atom+xml\" />");
    sb.AppendLine($"  <id>{selfUrl}</id>");
    sb.AppendLine($"  <updated>{feedUpdated:yyyy-MM-ddTHH:mm:ssZ}</updated>");
    sb.AppendLine("  <author><name>Dmitry Karataev</name></author>");

    foreach (var a in sorted)
    {
        var updated = AtomDate(a);
        var entryUrl = siteOrigin + writingBase.TrimEnd('/') + "/" + a.Slug + ".html";
        sb.AppendLine("  <entry>");
        sb.AppendLine($"    <title>{XmlText(a.Title)}</title>");
        sb.AppendLine($"    <link href=\"{entryUrl}\" rel=\"alternate\" type=\"text/html\" />");
        sb.AppendLine($"    <id>{entryUrl}</id>");
        sb.AppendLine($"    <updated>{updated:yyyy-MM-ddTHH:mm:ssZ}</updated>");
        sb.AppendLine($"    <summary type=\"text\">{XmlText(a.Description)}</summary>");
        sb.AppendLine("  </entry>");
    }

    sb.AppendLine("</feed>");
    File.WriteAllText(Path.Combine(outDir, "atom.xml"), sb.ToString(), new UTF8Encoding(false));
}

static DateTimeOffset AtomDate(Article a) =>
    new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero).AddDays(a.Order);

static string XmlText(string s)
{
    if (string.IsNullOrEmpty(s)) return "";
    return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

static string Indent(string html, string prefix)
{
    var lines = html.Split('\n');
    var sb = new StringBuilder();
    foreach (var line in lines)
    {
        if (sb.Length > 0) sb.AppendLine();
        sb.Append(string.IsNullOrWhiteSpace(line) ? line : prefix + line);
    }
    return sb.ToString();
}

static string BuildProvenanceAside(CoauthorshipProvenance p, bool isEn)
{
    var summary = isEn ? "How this piece was written" : "Как написан этот текст";
    var lblAuthor = isEn ? "Author" : "Автор";
    var lblCoauthors = isEn ? "Co-authors (tools / models)" : "Соавторы (инструменты / модели)";
    var lblContribution = isEn ? "Contribution" : "Вклад";
    var lblHumanFinal = isEn ? "Final review and responsibility" : "Финальная проверка и ответственность";

    var sb = new StringBuilder();
    sb.AppendLine("<aside class=\"article-provenance\" aria-label=\"" + WebUtility.HtmlEncode(summary) + "\">");
    sb.AppendLine("  <details class=\"article-provenance-details\">");
    sb.AppendLine("    <summary class=\"article-provenance-summary\">" + WebUtility.HtmlEncode(summary) + "</summary>");
    sb.AppendLine("    <dl class=\"article-provenance-dl\">");
    if (p.Author is { } au)
    {
        sb.AppendLine("      <dt>" + WebUtility.HtmlEncode(lblAuthor) + "</dt>");
        sb.AppendLine("      <dd>" + WebUtility.HtmlEncode(au) + "</dd>");
    }
    if (p.Coauthors.Count > 0)
    {
        sb.AppendLine("      <dt>" + WebUtility.HtmlEncode(lblCoauthors) + "</dt>");
        sb.AppendLine("      <dd><ul class=\"article-provenance-list\">");
        foreach (var c in p.Coauthors)
            sb.AppendLine("        <li>" + WebUtility.HtmlEncode(c) + "</li>");
        sb.AppendLine("      </ul></dd>");
    }
    if (p.Contribution is { } ct)
    {
        sb.AppendLine("      <dt>" + WebUtility.HtmlEncode(lblContribution) + "</dt>");
        sb.AppendLine("      <dd class=\"article-provenance-multiline\">" + WebUtility.HtmlEncode(ct) + "</dd>");
    }
    if (p.HumanFinal is { } hf)
    {
        sb.AppendLine("      <dt>" + WebUtility.HtmlEncode(lblHumanFinal) + "</dt>");
        sb.AppendLine("      <dd class=\"article-provenance-multiline\">" + WebUtility.HtmlEncode(hf) + "</dd>");
    }
    sb.AppendLine("    </dl>");
    sb.AppendLine("  </details>");
    sb.AppendLine("</aside>");
    return sb.ToString().TrimEnd();
}

record ArticleTag(string Display, string Slug);

record Article(string Slug, string Title, string Description, string DateDisplay, int Order, string BodyHtml, IReadOnlyList<ArticleTag> Tags, CoauthorshipProvenance? Provenance);

record CoauthorshipProvenance(string? Author, IReadOnlyList<string> Coauthors, string? Contribution, string? HumanFinal);

class FrontMatter
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Date_display { get; set; } = "";
    public int Order { get; set; } = 100;
    [YamlMember(Alias = "tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>When true and at least one provenance field is set, a disclosure block is rendered. Omit or false: no block (default).</summary>
    public bool Show_provenance { get; set; }

    public string? Provenance_author { get; set; }
    public List<string>? Provenance_coauthors { get; set; }
    public string? Provenance_contribution { get; set; }
    public string? Provenance_human_final { get; set; }
}

record NavLabels(
    string SkipToContent,
    string NavMain,
    string Language,
    string About,
    string Projects,
    string Writing,
    string Experience,
    string Docs,
    string AllWriting,
    string Email,
    string Telegram,
    string GitHub,
    string AllTags);
