using System.Text.Json.Nodes;
using Tomlyn;
using Tomlyn.Model;

namespace AIGuiders.DotnetTools.TomlCheck;

internal sealed class TomlCheckEngine(string? schemaOverride)
{
    public int CheckPaths(IEnumerable<string> paths)
    {
        var files = ExpandPaths(paths).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToList();
        if (files.Count == 0)
        {
            Console.Error.WriteLine("No TOML files matched.");
            return 1;
        }

        var failed = 0;
        foreach (var file in files)
        {
            if (!CheckFile(file))
                failed++;
        }

        if (failed > 0)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine($"{failed} file(s) failed validation.");
            return 1;
        }

        Console.WriteLine($"OK: {files.Count} file(s).");
        return 0;
    }

    private bool CheckFile(string tomlPath)
    {
        var schemaPath = ResolveSchemaPath(tomlPath);
        if (schemaPath is null)
        {
            Console.Error.WriteLine($"{tomlPath}: no #:schema directive (pass --schema <file.json>).");
            return false;
        }

        if (!File.Exists(schemaPath))
        {
            Console.Error.WriteLine($"{tomlPath}: schema not found: {schemaPath}");
            return false;
        }

        string text;
        try
        {
            text = File.ReadAllText(tomlPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{tomlPath}: read failed — {ex.Message}");
            return false;
        }

        TomlTable table;
        try
        {
            table = TomlSerializer.Deserialize<TomlTable>(text)
                ?? throw new InvalidOperationException("Empty TOML document.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{tomlPath}: TOML parse error — {ex.Message}");
            return false;
        }

        JsonNode document;
        try
        {
            document = TomlToJsonConverter.ToJsonNode(table);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{tomlPath}: TOML → JSON conversion failed — {ex.Message}");
            return false;
        }

        JsonSchemaValidator validator;
        try
        {
            validator = new JsonSchemaValidator(schemaPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{tomlPath}: schema load failed — {ex.Message}");
            return false;
        }

        if (validator.TryValidate(document, out var errors))
        {
            Console.WriteLine($"OK  {tomlPath}");
            return true;
        }

        Console.Error.WriteLine($"FAIL {tomlPath} (schema: {schemaPath})");
        foreach (var error in errors)
            Console.Error.WriteLine($"  {error}");
        return false;
    }

    private string? ResolveSchemaPath(string tomlPath)
    {
        if (!string.IsNullOrWhiteSpace(schemaOverride))
            return Path.GetFullPath(schemaOverride!);

        var directive = SchemaDirective.TryReadSchemaPath(tomlPath);
        return directive is null
            ? null
            : SchemaDirective.ResolveSchemaPath(tomlPath, directive);
    }

    private static IEnumerable<string> ExpandPaths(IEnumerable<string> paths)
    {
        foreach (var raw in paths)
        {
            var path = Path.GetFullPath(raw);
            if (File.Exists(path))
            {
                if (path.EndsWith(".toml", StringComparison.OrdinalIgnoreCase))
                    yield return path;
                continue;
            }

            if (!Directory.Exists(path))
            {
                Console.Error.WriteLine($"Path not found: {path}");
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(path, "*.toml", SearchOption.AllDirectories))
                yield return file;
        }
    }
}
