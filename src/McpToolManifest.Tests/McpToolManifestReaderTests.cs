using McpToolManifest;
using Xunit;

namespace McpToolManifest.Tests;

public sealed class McpToolManifestReaderTests
{
    [Fact]
    public void Load_and_Validate_sample_manifest()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "sample.manifest.json");
        var doc = McpToolManifestReader.Load(path);
        var errors = McpToolManifestReader.Validate(doc);
        Assert.Empty(errors);
        Assert.Equal("sample-mcp", doc.McpId);
        Assert.Equal(2, doc.Tools.Count);
    }

    [Fact]
    public void Validate_rejects_duplicate_tool_names()
    {
        var doc = new McpToolManifestDocument
        {
            SchemaVersion = 1,
            McpId = "x",
            Tools =
            [
                new McpToolManifestTool { Name = "a" },
                new McpToolManifestTool { Name = "a" }
            ]
        };
        var errors = McpToolManifestReader.Validate(doc);
        Assert.Contains(errors, e => e.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Compare_reports_catalog_manifest_mismatch()
    {
        var diff = ToolCatalogNameComparer.Compare(["a", "b"], ["a", "c"]);
        Assert.Equal(2, diff.Count);
        Assert.Contains(diff, s => s.Contains("not in catalog: b"));
        Assert.Contains(diff, s => s.Contains("not in manifest: c"));
    }
}
