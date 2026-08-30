using System.Text.Json.Serialization;

namespace McpToolManifest;

/// <summary>Корень файла <c>mcp-tools.manifest.json</c> у конкретного MCP (не общий протокол на весь монорепо).</summary>
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

/// <summary>Один инструмент: имя обязательно; описание опционально (для людей/доков; схема args пока в коде).</summary>
public sealed class McpToolManifestTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
