using System.Text.Json;

namespace McpToolManifest;

public static class McpToolManifestReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static McpToolManifestDocument Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.", nameof(path));
        if (!File.Exists(path))
            throw new FileNotFoundException("Manifest not found.", path);

        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<McpToolManifestDocument>(json, Options);
        if (doc is null)
            throw new InvalidDataException("Manifest deserialized to null.");

        return doc;
    }

    /// <summary>Возвращает ошибки или пустой список, если структура валидна.</summary>
    public static IReadOnlyList<string> Validate(McpToolManifestDocument doc, int expectedSchemaVersion = 1)
    {
        var errors = new List<string>();
        if (doc.SchemaVersion != expectedSchemaVersion)
            errors.Add($"schema_version: expected {expectedSchemaVersion}, got {doc.SchemaVersion}.");

        if (string.IsNullOrWhiteSpace(doc.McpId))
            errors.Add("mcp_id: required non-empty string.");

        if (doc.Tools.Count == 0)
            errors.Add("tools: at least one tool required.");

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var t in doc.Tools)
        {
            if (string.IsNullOrWhiteSpace(t.Name))
            {
                errors.Add("tools: entry with empty name.");
                continue;
            }

            var n = t.Name.Trim();
            if (!seen.Add(n))
                errors.Add($"tools: duplicate name '{n}'.");
        }

        return errors;
    }
}
