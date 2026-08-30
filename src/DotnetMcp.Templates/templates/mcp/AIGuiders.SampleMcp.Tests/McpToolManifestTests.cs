using McpToolManifest;

namespace AIGuiders.SampleMcp.Tests;

public sealed class McpToolManifestTests
{
    [Fact]
    public void Mcp_tools_manifest_matches_ToolCatalog_names_and_descriptions()
    {
        var manifestPath = Path.Combine(AppContext.BaseDirectory, "mcp-tools.manifest.json");
        Assert.True(File.Exists(manifestPath), $"Missing copied manifest: {manifestPath}");

        var doc = McpToolManifestReader.Load(manifestPath);
        var validation = McpToolManifestReader.Validate(doc);
        Assert.True(validation.Count == 0, string.Join(Environment.NewLine, validation));

        Assert.Equal("sample-mcp", doc.McpId);

        var catalog = ToolCatalog.Build()
            .ToDictionary(t => t.Name!, t => t.Description, StringComparer.Ordinal);

        var nameDiff = ToolCatalogNameComparer.Compare(doc.Tools.Select(t => t.Name), catalog.Keys);
        Assert.True(nameDiff.Count == 0, string.Join(Environment.NewLine, nameDiff));

        var manifestByName = doc.Tools.ToDictionary(t => t.Name, t => t.Description, StringComparer.Ordinal);
        Assert.Equal(catalog.Count, manifestByName.Count);
        foreach (var (name, expectedDesc) in catalog)
            Assert.Equal(expectedDesc, manifestByName[name]);
    }
}

