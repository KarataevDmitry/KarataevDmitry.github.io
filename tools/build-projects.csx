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

var srcEn = Path.Combine(repoRoot, "src", "projects", "en");
var srcRu = Path.Combine(repoRoot, "src", "projects", "ru");
var outEn = Path.Combine(repoRoot, "docs", "projects");
var outRu = Path.Combine(repoRoot, "docs", "ru", "projects");

if (!Directory.Exists(srcEn) || !Directory.Exists(srcRu))
{
    Console.Error.WriteLine("Expected src/projects/en and src/projects/ru under: " + repoRoot);
    Environment.Exit(1);
}

Directory.CreateDirectory(outEn);
Directory.CreateDirectory(outRu);

var mdPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
var yaml = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .IgnoreUnmatchedProperties()
    .Build();

var enItems = LoadProjects(srcEn, yaml, mdPipeline);
var ruItems = LoadProjects(srcRu, yaml, mdPipeline);

WriteIndex(enItems.OrderBy(x => x.Order).ThenBy(x => x.Slug).ToList(), "en", outEn);
WriteIndex(ruItems.OrderBy(x => x.Order).ThenBy(x => x.Slug).ToList(), "ru", outRu);

foreach (var p in enItems)
    WriteProjectPage(p, enItems, "en", outEn);
foreach (var p in ruItems)
    WriteProjectPage(p, ruItems, "ru", outRu);

Console.WriteLine($"Projects: {enItems.Count} EN + {ruItems.Count} RU → docs/projects/");

static List<ProjectPage> LoadProjects(string dir, IDeserializer yaml, MarkdownPipeline mdPipeline)
{
    var list = new List<ProjectPage>();
    foreach (var path in Directory.EnumerateFiles(dir, "*.md"))
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var (front, body) = SplitFrontMatter(text);
        var meta = yaml.Deserialize<ProjectFrontMatter>(front) ?? throw new InvalidOperationException("YAML: " + path);
        if (string.IsNullOrWhiteSpace(meta.Slug) || string.IsNullOrWhiteSpace(meta.Title))
            throw new InvalidOperationException("slug/title required: " + path);

        var html = Markdown.ToHtml(body.Trim(), mdPipeline);
        list.Add(new ProjectPage(
            meta.Slug,
            meta.Title,
            meta.Description ?? "",
            meta.Subtitle ?? "",
            meta.Repo_url ?? "",
            meta.Order,
            html));
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

static void WriteIndex(List<ProjectPage> projects, string lang, string outDir)
{
    var isEn = lang == "en";
    var home = isEn ? "/" : "/ru/";
    var basePath = isEn ? "/projects/" : "/ru/projects/";
    var pageTitle = isEn ? "Projects — Dmitry Karataev" : "Проекты — Дмитрий Каратаев";
    var pageDesc = isEn
        ? "Project pages with feature matrices for core MCP and IDE projects."
        : "Страницы проектов с таблицами возможностей для ключевых MCP и IDE проектов.";
    var h1 = isEn ? "Project Pages" : "Страницы проектов";
    var intro = isEn
        ? "Feature-oriented pages for core projects. Each page includes a capability matrix and links to implementation docs."
        : "Технические страницы ключевых проектов с таблицей возможностей и ссылками на реализацию.";
    var solutionsPrefix = isEn ? "Solutions" : "Решения";
    var solutionsAll = isEn ? "All" : "Все";
    var back = isEn ? "Back to homepage projects" : "Назад к проектам на главной";
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
    sb.AppendLine("  <link rel=\"alternate\" hreflang=\"en\" href=\"/projects/\" />");
    sb.AppendLine("  <link rel=\"alternate\" hreflang=\"ru\" href=\"/ru/projects/\" />");
    sb.AppendLine("  <link rel=\"alternate\" hreflang=\"x-default\" href=\"/projects/\" />");
    sb.AppendLine("  <link rel=\"icon\" href=\"data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>&#x1F4BB;</text></svg>\" />");
    sb.AppendLine("  <link rel=\"stylesheet\" href=\"/assets/css/articles.css\" />");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");
    sb.AppendLine($"  <a href=\"#main\" class=\"skip-link\">{(isEn ? "Skip to content" : "К содержанию")}</a>");
    sb.AppendLine($"  <nav aria-label=\"{(isEn ? "Main navigation" : "Основная навигация")}\">");
    sb.AppendLine("    <div class=\"nav-inner\">");
    sb.AppendLine($"      <div class=\"logo\"><a href=\"{home}\">{logo}</a></div>");
    sb.AppendLine("      <div class=\"nav-right\">");
    sb.AppendLine("        <ul>");
    sb.AppendLine($"          <li><a href=\"{home}#about\">{(isEn ? "About" : "О себе")}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#projects\" aria-current=\"page\">{(isEn ? "Projects" : "Проекты")}</a></li>");
    sb.AppendLine($"          <li><a href=\"{(isEn ? "/writing/" : "/ru/writing/")}\">{(isEn ? "Writing" : "Тексты")}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#experience\">{(isEn ? "Experience" : "Опыт")}</a></li>");
    sb.AppendLine($"          <li><a href=\"{home}#documents\">{(isEn ? "Docs" : "Документы")}</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine($"        <ul class=\"nav-lang\" aria-label=\"{(isEn ? "Language" : "Язык")}\">");
    sb.AppendLine($"          <li><a href=\"/projects/\" lang=\"en\"{(isEn ? " aria-current=\"page\"" : "")}>EN</a></li>");
    sb.AppendLine("          <li class=\"nav-sep\" aria-hidden=\"true\">|</li>");
    sb.AppendLine($"          <li><a href=\"/ru/projects/\" lang=\"ru\"{(!isEn ? " aria-current=\"page\"" : "")}>RU</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine("      </div>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </nav>");
    sb.AppendLine("  <main id=\"main\" class=\"container\">");
    sb.AppendLine("    <div class=\"article-wrap\">");
    sb.AppendLine($"      <p class=\"article-back\"><a href=\"{home}#projects\">&larr; {back}</a></p>");
    sb.AppendLine($"      <h1 class=\"page-title\">{WebUtility.HtmlEncode(h1)}</h1>");
    sb.AppendLine($"      <p class=\"page-intro\">{WebUtility.HtmlEncode(intro)}</p>");
    sb.AppendLine("      <p class=\"solutions-menu\">");
    sb.AppendLine(RenderProjectMenu(projects, basePath, "", solutionsPrefix, solutionsAll));
    sb.AppendLine("      </p>");
    sb.AppendLine("      <ul class=\"writing-list\">");
    foreach (var p in projects)
    {
        sb.AppendLine("        <li>");
        sb.AppendLine($"          <a class=\"title\" href=\"{basePath}{p.Slug}.html\">{WebUtility.HtmlEncode(p.Title)}</a>");
        sb.AppendLine($"          <p class=\"blurb\">{WebUtility.HtmlEncode(p.Description)}</p>");
        sb.AppendLine("        </li>");
    }
    sb.AppendLine("      </ul>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </main>");
    sb.AppendLine("  <footer id=\"contact\">");
    sb.AppendLine("    <div class=\"container\">");
    sb.AppendLine("      <div class=\"contact-links\">");
    sb.AppendLine("        <a href=\"mailto:dkarataev1990@gmail.com\">Email</a>");
    sb.AppendLine("        <a href=\"https://t.me/Krawler\">Telegram</a>");
    sb.AppendLine("        <a href=\"https://github.com/KarataevDmitry\">GitHub</a>");
    sb.AppendLine("      </div>");
    sb.AppendLine($"      <p>{siteName} &copy; 2026.</p>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </footer>");
    sb.AppendLine("</body>");
    sb.AppendLine("</html>");

    File.WriteAllText(Path.Combine(outDir, "index.html"), sb.ToString(), new UTF8Encoding(false));
}

static void WriteProjectPage(ProjectPage p, List<ProjectPage> allProjects, string lang, string outDir)
{
    var isEn = lang == "en";
    var home = isEn ? "/" : "/ru/";
    var basePath = isEn ? "/projects/" : "/ru/projects/";
    var enUrl = "/projects/" + p.Slug + ".html";
    var ruUrl = "/ru/projects/" + p.Slug + ".html";
    var pageTitle = p.Title + (isEn ? " — Projects" : " — Проекты");
    var solutionsPrefix = isEn ? "Solutions" : "Решения";
    var solutionsAll = isEn ? "All" : "Все";
    var logo = isEn ? "D<span>.</span>Karataev" : "Д<span>.</span>Каратаев";
    var siteName = isEn ? "Dmitry Karataev" : "Дмитрий Каратаев";

    var sb = new StringBuilder();
    sb.AppendLine("<!DOCTYPE html>");
    sb.AppendLine($"<html lang=\"{lang}\">");
    sb.AppendLine("<head>");
    sb.AppendLine("  <meta charset=\"utf-8\" />");
    sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
    sb.AppendLine($"  <title>{WebUtility.HtmlEncode(pageTitle)}</title>");
    sb.AppendLine($"  <meta name=\"description\" content=\"{WebUtility.HtmlEncode(p.Description)}\" />");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"en\" href=\"{enUrl}\" />");
    sb.AppendLine($"  <link rel=\"alternate\" hreflang=\"ru\" href=\"{ruUrl}\" />");
    sb.AppendLine("  <link rel=\"icon\" href=\"data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><text y='.9em' font-size='90'>&#x1F4BB;</text></svg>\" />");
    sb.AppendLine("  <link rel=\"stylesheet\" href=\"/assets/css/articles.css\" />");
    sb.AppendLine("  <style>.prose table{width:100%;border-collapse:collapse;margin:1rem 0}.prose th,.prose td{border:1px solid #334155;padding:.55rem;text-align:left}.prose th{color:#f1f5f9;background:#1e293b}</style>");
    sb.AppendLine("</head>");
    sb.AppendLine("<body>");
    sb.AppendLine($"  <a href=\"#main\" class=\"skip-link\">{(isEn ? "Skip to content" : "К содержанию")}</a>");
    sb.AppendLine($"  <nav aria-label=\"{(isEn ? "Main navigation" : "Основная навигация")}\">");
    sb.AppendLine("    <div class=\"nav-inner\">");
    sb.AppendLine($"      <div class=\"logo\"><a href=\"{home}\">{logo}</a></div>");
    sb.AppendLine("      <div class=\"nav-right\">");
    sb.AppendLine("        <ul>");
    sb.AppendLine($"          <li><a href=\"{home}#projects\" aria-current=\"page\">{(isEn ? "Projects" : "Проекты")}</a></li>");
    sb.AppendLine($"          <li><a href=\"{(isEn ? "/writing/" : "/ru/writing/")}\">{(isEn ? "Writing" : "Тексты")}</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine($"        <ul class=\"nav-lang\" aria-label=\"{(isEn ? "Language" : "Язык")}\">");
    sb.AppendLine($"          <li><a href=\"{enUrl}\" lang=\"en\"{(isEn ? " aria-current=\"page\"" : "")}>EN</a></li>");
    sb.AppendLine("          <li class=\"nav-sep\" aria-hidden=\"true\">|</li>");
    sb.AppendLine($"          <li><a href=\"{ruUrl}\" lang=\"ru\"{(!isEn ? " aria-current=\"page\"" : "")}>RU</a></li>");
    sb.AppendLine("        </ul>");
    sb.AppendLine("      </div>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </nav>");
    sb.AppendLine("  <main id=\"main\" class=\"container\">");
    sb.AppendLine("    <article class=\"article-wrap\">");
    sb.AppendLine($"      <p class=\"article-back\"><a href=\"{basePath}\">&larr; {(isEn ? "All projects" : "Все проекты")}</a></p>");
    sb.AppendLine("      <header class=\"article-header\">");
    sb.AppendLine($"        <h1>{WebUtility.HtmlEncode(p.Title)}</h1>");
    if (!string.IsNullOrWhiteSpace(p.Subtitle))
        sb.AppendLine($"        <p class=\"article-meta\">{WebUtility.HtmlEncode(p.Subtitle)}</p>");
    sb.AppendLine("        <p class=\"solutions-menu\">");
    sb.AppendLine(RenderProjectMenu(allProjects, basePath, p.Slug, solutionsPrefix, solutionsAll));
    sb.AppendLine("        </p>");
    sb.AppendLine("      </header>");
    sb.AppendLine("      <div class=\"prose\">");
    sb.AppendLine(Indent(p.BodyHtml, "        "));
    if (!string.IsNullOrWhiteSpace(p.RepoUrl))
        sb.AppendLine($"        <p><strong>{(isEn ? "Repository" : "Репозиторий")}:</strong> <a href=\"{WebUtility.HtmlEncode(p.RepoUrl)}\">{WebUtility.HtmlEncode(p.RepoUrl)}</a></p>");
    sb.AppendLine("      </div>");
    sb.AppendLine("    </article>");
    sb.AppendLine("  </main>");
    sb.AppendLine("  <footer id=\"contact\">");
    sb.AppendLine("    <div class=\"container\">");
    sb.AppendLine("      <div class=\"contact-links\">");
    sb.AppendLine("        <a href=\"mailto:dkarataev1990@gmail.com\">Email</a>");
    sb.AppendLine("        <a href=\"https://t.me/Krawler\">Telegram</a>");
    sb.AppendLine("        <a href=\"https://github.com/KarataevDmitry\">GitHub</a>");
    sb.AppendLine("      </div>");
    sb.AppendLine($"      <p>{siteName} &copy; 2026.</p>");
    sb.AppendLine("    </div>");
    sb.AppendLine("  </footer>");
    sb.AppendLine("</body>");
    sb.AppendLine("</html>");

    File.WriteAllText(Path.Combine(outDir, p.Slug + ".html"), sb.ToString(), new UTF8Encoding(false));
}

static string Indent(string html, string prefix)
{
    var lines = html.Split('\n');
    var sb = new StringBuilder();
    foreach (var line in lines)
    {
        if (sb.Length > 0)
            sb.AppendLine();
        sb.Append(string.IsNullOrWhiteSpace(line) ? line : prefix + line);
    }

    return sb.ToString();
}

static string RenderProjectMenu(List<ProjectPage> projects, string basePath, string currentSlug, string prefix, string allLabel)
{
    var sb = new StringBuilder();
    sb.Append($"<a href=\"{basePath}\"");
    if (string.IsNullOrEmpty(currentSlug))
        sb.Append(" aria-current=\"page\"");
    sb.Append($">{WebUtility.HtmlEncode(prefix)} &gt; {WebUtility.HtmlEncode(allLabel)}</a> ");

    foreach (var item in projects.OrderBy(x => x.Order).ThenBy(x => x.Slug))
    {
        sb.Append($"<a href=\"{basePath}{item.Slug}.html\"");
        if (item.Slug == currentSlug)
            sb.Append(" aria-current=\"page\"");
        sb.Append(">");
        sb.Append(WebUtility.HtmlEncode(prefix));
        sb.Append(" &gt; ");
        sb.Append(WebUtility.HtmlEncode(item.Title));
        sb.Append("</a> ");
    }
    return sb.ToString().TrimEnd();
}

record ProjectPage(string Slug, string Title, string Description, string Subtitle, string RepoUrl, int Order, string BodyHtml);

class ProjectFrontMatter
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Repo_url { get; set; } = "";
    public int Order { get; set; } = 100;
}
