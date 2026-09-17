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
            return CheckWithDocumentDiscovery(tomlPath);

        if (!File.Exists(schemaPath))
        {
            Console.Error.WriteLine($"{tomlPath}: schema not found: {schemaPath}");
            return false;
        }

        return IsTomlSchemaPath(schemaPath)
            ? CheckWithTomlSchema(tomlPath, schemaPath)
            : CheckWithJsonSchema(tomlPath, schemaPath);
    }

    private bool CheckWithDocumentDiscovery(string tomlPath)
    {
        try
        {
            var result = TomlSchema.TomlSchema.ValidateDocument(tomlPath);
            return ReportTomlSchemaResult(tomlPath, "[toml-schema].location", result);
        }
        catch (TomlSchema.SchemaException ex)
        {
            Console.Error.WriteLine($"{tomlPath}: schema discovery failed — {ex.Message}");
            return false;
        }
        catch (TomlSchema.DocumentParseException ex)
        {
            Console.Error.WriteLine($"{tomlPath}: TOML parse error — {ex.Message}");
            return false;
        }
    }

    private bool CheckWithTomlSchema(string tomlPath, string schemaPath)
    {
        try
        {
            var schema = TomlSchema.TomlSchema.Load(schemaPath);
            var result = schema.Validate(tomlPath);
            return ReportTomlSchemaResult(tomlPath, schemaPath, result);
        }
        catch (TomlSchema.SchemaException ex)
        {
            Console.Error.WriteLine($"{tomlPath}: schema load failed — {ex.Message}");
            return false;
        }
        catch (TomlSchema.DocumentParseException ex)
        {
            Console.Error.WriteLine($"{tomlPath}: TOML parse error — {ex.Message}");
            return false;
        }
    }

    private bool CheckWithJsonSchema(string tomlPath, string schemaPath)
    {
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

    private static bool ReportTomlSchemaResult(string tomlPath, string schemaLabel, TomlSchema.ValidationResult result)
    {
        foreach (var warning in result.Warnings)
            Console.Error.WriteLine($"WARN {tomlPath}: {warning}");

        if (result.IsValid)
        {
            Console.WriteLine($"OK  {tomlPath}");
            return true;
        }

        Console.Error.WriteLine($"FAIL {tomlPath} (schema: {schemaLabel})");
        foreach (var error in result.Errors)
            Console.Error.WriteLine($"  {error}");
        return false;
    }

    private string? ResolveSchemaPath(string tomlPath)
    {
        if (!string.IsNullOrWhiteSpace(schemaOverride))
            return Path.GetFullPath(schemaOverride!);

        var directive = SchemaDirective.TryReadSchemaPath(tomlPath);
        if (directive is not null)
            return SchemaDirective.ResolveSchemaPath(tomlPath, directive);

        return null;
    }

    private static bool IsTomlSchemaPath(string schemaPath) =>
        schemaPath.EndsWith(".tosd", StringComparison.OrdinalIgnoreCase);

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
