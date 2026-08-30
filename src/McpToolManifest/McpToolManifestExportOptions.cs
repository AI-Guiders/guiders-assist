namespace McpToolManifest;

/// <summary>Metadata for <see cref="McpToolManifestExporter"/> (per-MCP repo).</summary>
public sealed class McpToolManifestExportOptions
{
    public required string McpId { get; init; }

    public required string Title { get; init; }

    /// <summary>Repo folder name under open/ (used in generated MCP-TOOLS.md hint).</summary>
    public required string RepoFolderName { get; init; }

    /// <summary>Optional extra line in MCP-TOOLS.md about inputSchema (defaults to list_tools hint).</summary>
    public string? SchemaHint { get; init; }
}
