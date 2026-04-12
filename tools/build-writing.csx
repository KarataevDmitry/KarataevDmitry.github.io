#!/usr/bin/env dotnet-script
#r "nuget: Markdig, 0.37.0"
#r "nuget: YamlDotNet, 16.3.0"

using System.Net;
using System.Text;
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

Console.WriteLine($"Writing: {enArticles.Count} EN + {ruArticles.Count} RU articles → docs/writing/");

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
        list.Add(new Article(meta.Slug, meta.Title, meta.Description ?? "", meta.Date_display ?? "", meta.Order, html));
    }
    return list;
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
        ? new NavLabels("Skip to content", "Main navigation", "Language", "About", "Projects", "Writing", "Experience", "Docs", "All writing", "Email", "Telegram", "GitHub")
        : new NavLabels("К содержанию", "Основная навигация", "Язык", "О себе", "Проекты", "Тексты", "Опыт", "Документы", "Все тексты", "Email", "Telegram", "GitHub");

    var home = isEn ? "/" : "/ru/";
    var fileName = a.Slug + ".html";
    var enUrl = "/writing/" + fileName;
    var ruUrl = "/ru/writing/" + fileName;
    var pageTitle = WebUtility.HtmlEncode(a.Title) + (isEn ? " — Dmitry Karataev" : " — Дмитрий Каратаев");
    var siteName = isEn ? "Dmitry Karataev" : "Дмитрий Каратаев";
    var logo = isEn ? "D<span>.</span>Karataev" : "Д<span>.</span>Каратаев";

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
    sb.AppendLine($"      <p class=\"article-back\"><a href=\"{writingBase}\">&larr; {labels.AllWriting}</a></p>");
    sb.AppendLine();
    sb.AppendLine("      <header class=\"article-header\">");
    sb.AppendLine($"        <h1>{WebUtility.HtmlEncode(a.Title)}</h1>");
    sb.AppendLine($"        <p class=\"article-meta\">{WebUtility.HtmlEncode(a.DateDisplay)}</p>");
    sb.AppendLine("      </header>");
    sb.AppendLine();
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
        ? new NavLabels("Skip to content", "Main navigation", "Language", "About", "Projects", "Writing", "Experience", "Docs", "All writing", "Email", "Telegram", "GitHub")
        : new NavLabels("К содержанию", "Основная навигация", "Язык", "О себе", "Проекты", "Тексты", "Опыт", "Документы", "Все тексты", "Email", "Telegram", "GitHub");

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
    sb.AppendLine();
    sb.AppendLine("      <ul class=\"writing-list\">");
    foreach (var a in articles)
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

    File.WriteAllText(Path.Combine(outDir, "index.html"), sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
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

record Article(string Slug, string Title, string Description, string DateDisplay, int Order, string BodyHtml);

class FrontMatter
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Date_display { get; set; } = "";
    public int Order { get; set; } = 100;
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
    string GitHub);
