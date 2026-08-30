using System.Text.Json.Serialization;

namespace McpToolManifest;

/// <summary>Root of per-MCP manifest file <c>mcp-tools.manifest.json</c>.</summary>
public sealed class McpToolManifestDocument
{
    [JsonPropertyName("schema_version")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("mcp_id")]
    public string McpId { get; set; } = "";

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("tools")]
    public List<McpToolManifestTool> Tools { get; set; } = [];
}

/// <summary>One tool entry. Name is required; description is optional.</summary>
public sealed class McpToolManifestTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

