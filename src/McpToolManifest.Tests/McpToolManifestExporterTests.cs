using McpToolManifest;
using Xunit;

namespace McpToolManifest.Tests;

public sealed class McpToolManifestExporterTests
{
    [Fact]
    public void Run_writes_json_and_markdown()
    {
        var dir = Path.Combine(Path.GetTempPath(), "mcp-export-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        try
        {
            var tools = new List<(string Name, string? Description)>
            {
                ("ping", "Health check"),
            };
            var options = new McpToolManifestExportOptions
            {
                McpId = "sample-mcp",
                Title = "Sample MCP",
                RepoFolderName = "sample-mcp",
            };

            var code = McpToolManifestExporter.Run(["--write"], tools, options, dir);
            Assert.Equal(0, code);
            Assert.True(File.Exists(Path.Combine(dir, "mcp-tools.manifest.json")));
            var md = File.ReadAllText(Path.Combine(dir, "docs", "MCP-TOOLS.md"));
            Assert.Contains("### `ping`", md);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }
}
