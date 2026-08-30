using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpToolManifest;

/// <summary>Shared export runner for MCP tool manifests (JSON + docs/MCP-TOOLS.md).</summary>
public static class McpToolManifestExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// <c>--write</c> → mcp-tools.manifest.json + docs/MCP-TOOLS.md;
    /// <c>--md-only</c> → markdown stdout;
    /// default → JSON document stdout.
    /// </summary>
    public static int Run(
        string[] args,
        IReadOnlyList<(string Name, string? Description)> tools,
        McpToolManifestExportOptions options,
        string? workingDirectory = null)
    {
        var root = workingDirectory ?? Directory.GetCurrentDirectory();
        var doc = BuildDocument(tools, options);

        if (ContainsFlag(args, "--write"))
        {
            var jsonPath = Path.Combine(root, "mcp-tools.manifest.json");
            var mdDir = Path.Combine(root, "docs");
            var mdPath = Path.Combine(mdDir, "MCP-TOOLS.md");
            Directory.CreateDirectory(mdDir);

            WriteJsonFile(jsonPath, doc);
            File.WriteAllText(mdPath, McpToolsDocMarkdown.Build(tools, options), Utf8NoBom);

            Console.Error.WriteLine($"Wrote {jsonPath}");
            Console.Error.WriteLine($"Wrote {mdPath}");
            return 0;
        }

        if (ContainsFlag(args, "--md-only"))
        {
            Console.WriteLine(McpToolsDocMarkdown.Build(tools, options).TrimEnd());
            return 0;
        }

        Console.WriteLine(JsonSerializer.Serialize(doc, JsonOptions));
        return 0;
    }

    public static McpToolManifestDocument BuildDocument(
        IReadOnlyList<(string Name, string? Description)> tools,
        McpToolManifestExportOptions options) =>
        new()
        {
            SchemaVersion = 1,
            McpId = options.McpId,
            Title = options.Title,
            Tools = tools.Select(t => new McpToolManifestTool { Name = t.Name, Description = t.Description }).ToList(),
        };

    public static void WriteJsonFile(string path, McpToolManifestDocument doc) =>
        File.WriteAllText(path, JsonSerializer.Serialize(doc, JsonOptions) + Environment.NewLine, Utf8NoBom);

    private static bool ContainsFlag(string[] args, string flag) =>
        args.Any(a => string.Equals(a, flag, StringComparison.Ordinal));
}
