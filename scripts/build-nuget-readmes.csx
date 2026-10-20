// Сборка NUGET.md (страниц пакетов на nuget.org) из фактов. Тул публичный: чужая раскладка может
// не совпадать с нашей — потому факты адресуют проекты явно (project:), а папки задаются параметрами.
// Запуск из корня целевой репы (или путь аргументом):
//   dotnet script <путь-к-guiders-assist>/scripts/build-nuget-readmes.csx              → регенерация NUGET.md
//   dotnet script ... --check                                                          → проверка без записи
//   dotnet script ... D:\path\to\repo                                                  → целевая репа аргументом (по умолчанию cwd)
//   dotnet script ... --facts my/docs/dir                                              → папка фактов (по умолчанию docs/nuget/packages)
// Нужно: dotnet tool install -g dotnet-script
// Факт: <PackageId>.md — заголовок (package:, tagline:, project:) + секции:
//   ## What you get / ## Quick start / ## Problem / ## Solution / ## When not to use / ## Learn more
// project: — необязательный явный путь к файлу проекта от корня репы — для раскладок не как у нас.
// .NET многоязычен: fsproj / csproj / vbproj. Без project: — конвенция src/<package>/<package>.fsproj|.csproj|.vbproj.
// Выход: NUGET.md рядом с файлом проекта — генерируемый артефакт (конвенция упаковки — Directory.Build.targets
// целевой репы). Правим факты и шаблон здесь — не выход; править можно всё что угодно и когда угодно.
// Подвал (MIT + ссылка на репо) штампуется тулом из git remote origin.
// EOL — git check-attr по каждому файлу: гит сам решает цепочку атрибутов
// (info/attributes → вложенные .gitattributes → глобальный attributesFile).
// Идея та же, что build-resume.csx: источник — данные, сборка — скрипт, артефакты регенерируемы.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

var utf8NoBom = new UTF8Encoding(false);
var requiredSections = new[]
{
    "What you get", "Quick start", "Problem", "Solution", "When not to use", "Learn more"
};

// --- аргументы ---
var repoRoot = Directory.GetCurrentDirectory();
var checkOnly = false;
var factsRel = "docs/nuget/packages";
{
    var argv = Environment.GetCommandLineArgs();
    var afterScript = new List<string>();
    var found = false;
    foreach (var a in argv)
    {
        if (found) afterScript.Add(a);
        else if (a.EndsWith(".csx", StringComparison.OrdinalIgnoreCase)
                 && a.Contains("build-nuget-readmes", StringComparison.OrdinalIgnoreCase))
            found = true;
    }
    // Два режима: dotnet script (аргументы после .csx) и скомпилированный exe-тул
    // (dotnet script не участвует: аргументы после argv[0]).
    if (!found) afterScript.AddRange(argv.Skip(1));
    for (var i = 0; i < afterScript.Count; i++)
    {
        var a = afterScript[i];
        if (a.Equals("--check", StringComparison.Ordinal)) { checkOnly = true; continue; }
        if (a.Equals("--facts", StringComparison.Ordinal) && i + 1 < afterScript.Count)
        {
            factsRel = afterScript[i + 1];
            i++;
            continue;
        }
        if (!a.StartsWith("-") && Directory.Exists(a)) { repoRoot = Path.GetFullPath(a); continue; }
        if (!a.StartsWith("-")) Console.WriteLine($"Warning: extra argument ignored: {a}");
    }
}

// --- git remote origin → https (для подвала) ---
var repoUrl = "";
{
    var psi = new ProcessStartInfo("git", "-C \"" + repoRoot + "\" remote get-url origin")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    using (var p = Process.Start(psi))
    {
        repoUrl = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
    }
    if (repoUrl.Length == 0)
    {
        Console.Error.WriteLine("ERROR: git remote get-url origin failed in " + repoRoot);
        Environment.Exit(1);
    }
    if (repoUrl.StartsWith("git@")) repoUrl = repoUrl.Replace(":", "/").Replace("git@", "https://");
    if (repoUrl.EndsWith(".git")) repoUrl = repoUrl.Substring(0, repoUrl.Length - 4);
}

// --- EOL от git: `git check-attr -z eol text -- <путь>` решает всю цепочку сам
// (info/attributes → вложенные .gitattributes → глобальный attributesFile) — своего
// парсинга .gitattributes нет, вложенные файлы учитываются по-гитовски точно.
// Разрешение — по каждому целевому файлу отдельно (в цикле фактов ниже).
// core.autocrlf (кэш) — только когда правило text есть, а eol не задан.
var autoCrlfCache = (string)null;
Func<string> autoCrlf =
    () =>
    {
        if (autoCrlfCache == null)
        {
            var cfg = new ProcessStartInfo("git", "-C \"" + repoRoot + "\" config core.autocrlf")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var p = Process.Start(cfg))
            {
                autoCrlfCache = p.StandardOutput.ReadToEnd().Trim().ToLowerInvariant();
                p.WaitForExit();
            }
            if (autoCrlfCache.Length == 0) autoCrlfCache = "unset";
        }
        return autoCrlfCache;
    };

var factsDir = Path.IsPathRooted(factsRel) ? factsRel : Path.Combine(repoRoot, factsRel);
var srcDir = Path.Combine(repoRoot, "src");
if (!Directory.Exists(factsDir)) { Console.Error.WriteLine("ERROR: no facts dir: " + factsDir); Environment.Exit(1); }
// src/ не обязателен: явные project: в фактах работают и без него; src/ нужен только конвенции
// (src/<package>/<package>.??proj) и отчёту покрытия.

// --- разбор фактов → рендер → запись ---
var failures = 0;
var generated = 0;
var unchangedCount = 0;
var covered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var factFiles = Directory.EnumerateFiles(factsDir, "*.md").OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
if (factFiles.Count == 0) Console.WriteLine("No fact files in " + factsDir);

foreach (var factPath in factFiles)
{
    var errors = new List<string>();
    var packageId = "";
    var tagline = "";
    var projectPath = "";
    var sectionOrder = new List<string>();
    var sectionLines = new Dictionary<string, List<string>>(StringComparer.Ordinal);
    string current = null;

    foreach (var line in File.ReadAllLines(factPath))
    {
        if (current == null && line.StartsWith("package: ", StringComparison.Ordinal))
        {
            packageId = line.Substring(9).Trim();
            continue;
        }
        if (current == null && line.StartsWith("tagline: ", StringComparison.Ordinal))
        {
            tagline = line.Substring(9).Trim();
            continue;
        }
        if (current == null && line.StartsWith("project: ", StringComparison.Ordinal))
        {
            projectPath = line.Substring(9).Trim();
            continue;
        }
        if (line.StartsWith("## ", StringComparison.Ordinal))
        {
            current = line.Substring(3).Trim();
            if (sectionLines.ContainsKey(current)) errors.Add($"duplicate section '{current}'");
            sectionOrder.Add(current);
            sectionLines[current] = new List<string>();
            continue;
        }
        if (current == null)
        {
            if (line.Trim().Length == 0) continue;
            errors.Add($"unexpected line before first section: '{line}'");
            continue;
        }
        sectionLines[current].Add(line);
    }

    if (packageId.Length == 0) errors.Add("missing 'package:' header");
    if (tagline.Length == 0) errors.Add("missing 'tagline:' header");
    var missing = requiredSections.Where(s => !sectionOrder.Contains(s, StringComparer.Ordinal)).ToList();
    if (missing.Count > 0) errors.Add("missing sections: " + string.Join(", ", missing));
    var unknown = sectionOrder.Where(s => !requiredSections.Contains(s, StringComparer.Ordinal)).ToList();
    if (unknown.Count > 0) errors.Add("unknown sections: " + string.Join(", ", unknown));

    // Где проект: явный project: в факте (путь от корня репы) или конвенция src/<package>/.
    // .NET многоязычен: fsproj / csproj / vbproj.
    var projectFile = "";
    if (projectPath.Length > 0)
    {
        if (Path.IsPathRooted(projectPath)) errors.Add("project: must be repo-relative (got absolute path)");
        else
        {
            projectFile = Path.GetFullPath(Path.Combine(repoRoot, projectPath));
            if (!File.Exists(projectFile)) { errors.Add("project: file not found: " + projectPath); projectFile = ""; }
        }
    }
    else if (packageId.Length > 0)
    {
        if (Directory.Exists(srcDir))
        {
            var convDir = Path.Combine(srcDir, packageId);
            projectFile = new[] { ".fsproj", ".csproj", ".vbproj" }
                .Select(ext => Path.Combine(convDir, packageId + ext))
                .FirstOrDefault(File.Exists) ?? "";
        }
        if (projectFile.Length == 0)
            errors.Add($"no project for '{packageId}': set project: (repo-relative path) or place src/{packageId}/{packageId}.fsproj|.csproj|.vbproj");
    }
    var projectDir = projectFile.Length > 0 ? (Path.GetDirectoryName(projectFile) ?? "") : "";

    if (errors.Count > 0)
    {
        foreach (var e in errors) Console.Error.WriteLine($"ERROR {Path.GetFileName(factPath)}: {e}");
        failures++;
        continue;
    }

    covered.Add(packageId);

    // --- eol этого файла: git check-attr (вся цепочка, включая вложенные .gitattributes).
    // Относительный путь вывода — от реального расположения проекта (явный project: или конвенция).
    var relTarget = projectDir.Substring(repoRoot.Length + 1).Replace('\\', '/') + "/NUGET.md";
    var attrPsi = new ProcessStartInfo("git", "-C \"" + repoRoot + "\" check-attr -z eol text -- \"" + relTarget + "\"")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    var eolValue = "unspecified";
    var textValue = "unspecified";
    {
        string attrRaw;
        using (var p = Process.Start(attrPsi))
        {
            attrRaw = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
        }
        // -z: записи path\0attr\0value\0 подряд
        var fields = attrRaw.Split('\0');
        for (var i = 0; i + 2 < fields.Length; i += 3)
        {
            if (fields[i + 1].Equals("eol", StringComparison.Ordinal)) eolValue = fields[i + 2];
            else if (fields[i + 1].Equals("text", StringComparison.Ordinal)) textValue = fields[i + 2];
        }
    }
    var newline = "\n";
    var eolSource = "no rule — bytes as written";
    if (eolValue.Equals("lf", StringComparison.Ordinal)) eolSource = "git eol=lf";
    else if (eolValue.Equals("crlf", StringComparison.Ordinal)) { newline = "\r\n"; eolSource = "git eol=crlf"; }
    else if (textValue.Equals("set", StringComparison.Ordinal) || textValue.Equals("auto", StringComparison.Ordinal))
    {
        // text есть, eol не задан: git решает core.autocrlf (true → CRLF в working tree)
        var ac = autoCrlf();
        if (ac.Contains("true")) { newline = "\r\n"; eolSource = "git text, autocrlf=true"; }
        else eolSource = "git text, autocrlf=" + ac;
    }

    // --- рендер: заголовок → секции в каноническом порядке → подвал ---
    var sb = new StringBuilder();
    sb.AppendLine("# " + packageId);
    sb.AppendLine();
    sb.AppendLine(tagline);
    sb.AppendLine();
    foreach (var s in requiredSections)
    {
        var body = sectionLines[s];
        while (body.Count > 0 && body[0].Trim().Length == 0) body.RemoveAt(0);
        while (body.Count > 0 && body[body.Count - 1].Trim().Length == 0) body.RemoveAt(body.Count - 1);
        sb.AppendLine("## " + s);
        sb.AppendLine();
        foreach (var l in body) sb.AppendLine(l);
        sb.AppendLine();
    }
    sb.AppendLine("---");
    sb.AppendLine();
    sb.AppendLine($"MIT · [Source & issues]({repoUrl}) · part of the Guiders federation");

    var target = Path.Combine(projectDir, "NUGET.md");
    // EOL — из git check-attr по этому файлу (разрешение выше): AppendLine платформы
    // канонизируем в LF, затем в EOL файла.
    var rendered = sb.ToString().Replace("\r\n", "\n").Replace("\n", newline);
    var existing = File.Exists(target) ? File.ReadAllText(target) : "";
    if (existing == rendered)
    {
        unchangedCount++;
        Console.WriteLine(packageId + ": unchanged (eol " + (newline == "\r\n" ? "crlf" : "lf") + ", " + eolSource + ")");
    }
    else
    {
        if (!checkOnly) File.WriteAllText(target, rendered, utf8NoBom);
        generated++;
        Console.WriteLine(packageId + ": " + (checkOnly ? "would regenerate" : "generated") + " (eol " + (newline == "\r\n" ? "crlf" : "lf") + ", " + eolSource + ")");
    }
}

// --- покрытие: пакеты без фактов (продолжают получать корневой README).
// Отчёт по конвенции src/<name>/<name>.fsproj|.csproj|.vbproj; раскладки с явными project:
// и без src/ честно помечаются — покрытие для них не считается.
Console.WriteLine();
if (Directory.Exists(srcDir))
{
    var projectIds = Directory.EnumerateDirectories(srcDir)
        .Select(d =>
        {
            var name = Path.GetFileName(d);
            return new[] { ".fsproj", ".csproj", ".vbproj" }.Any(ext => File.Exists(Path.Combine(d, name + ext)))
                ? name : null;
        })
        .Where(n => n != null)
        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
        .ToList();

    var noFacts = projectIds.Where(id => !covered.Contains(id)).ToList();
    Console.WriteLine($"packages: {projectIds.Count} · with NUGET.md: {covered.Count} · root README: {noFacts.Count}");
    foreach (var id in noFacts)
        Console.WriteLine("  " + id + ": no fact file (ships root README)");
}
else
{
    Console.WriteLine($"facts: {covered.Count} · coverage: no src/ dir — explicit project: layout, coverage skipped");
}

if (failures > 0)
{
    Console.Error.WriteLine($"FAILED: {failures} fact file(s) with errors");
    Environment.Exit(1);
}
Console.WriteLine(checkOnly ? "check ok" : $"done: {generated} generated, {unchangedCount} unchanged");
