using System.Text;

namespace McpToolManifest;

public static class McpToolsDocMarkdown
{
    public static string Build(
        IEnumerable<(string Name, string? Description)> tools,
        McpToolManifestExportOptions options)
    {
        var schemaHint = options.SchemaHint
            ?? "Тексты совпадают с полем `description` у инструментов MCP; полная схема аргументов — в `inputSchema` (например через `list_tools`).";

        var sb = new StringBuilder();
        sb.AppendLine($"# {options.Title} — каталог тулов");
        sb.AppendLine();
        sb.AppendLine("<!-- GENERATED:ToolCatalog START -->");
        sb.AppendLine();
        sb.AppendLine("> Автогенерация из `ToolCatalog.Build()` в репозитории. Не править этот блок вручную.");
        sb.AppendLine(">");
        sb.AppendLine($"> Обновление: из каталога `{options.RepoFolderName}` выполнить `dotnet run --project tools/ExportMcpManifest -- --write`.");
        sb.AppendLine(">");
        sb.AppendLine($"> {schemaHint}");
        sb.AppendLine();

        foreach (var (name, description) in tools)
        {
            sb.AppendLine($"### `{name}`");
            sb.AppendLine();
            sb.AppendLine((description ?? string.Empty).TrimEnd());
            sb.AppendLine();
        }

        sb.AppendLine("<!-- GENERATED:ToolCatalog END -->");
        sb.AppendLine();
        return sb.ToString();
    }
}
