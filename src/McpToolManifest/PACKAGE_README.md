# AIGuiders.McpToolManifest

Библиотека для **per-MCP** файла `mcp-tools.manifest.json`: загрузка JSON, проверка структуры (`schema_version`, `mcp_id`, уникальные `tools[].name`), сравнение имён манифеста с рантайм-каталогом тулов.

Без зависимости от MCP SDK — только `System.Text.Json`.

## Установка

```bash
dotnet add package AIGuiders.McpToolManifest
```

**Ссылки:** [NuGet.org](https://www.nuget.org/packages/AIGuiders.McpToolManifest) · [Исходники](https://github.com/KarataevDmitry/mcp-tool-manifest) · [MIT](LICENSE).

## Пример

```csharp
using McpToolManifest;

var doc = McpToolManifestReader.Load("mcp-tools.manifest.json");
var errors = McpToolManifestReader.Validate(doc);
if (errors.Count > 0)
    throw new InvalidOperationException(string.Join(Environment.NewLine, errors));

var diff = ToolCatalogNameComparer.Compare(
    doc.Tools.Select(t => t.Name),
    catalogToolNames);
```

## Формат JSON

- `schema_version` — сейчас `1`
- `mcp_id` — стабильный id сервера (например `agent-notes-mcp`)
- `tools[]` — `{ "name", "description?" }`

Генерация манифеста из `ToolCatalog` в конкретном MCP — отдельный проект `ExportMcpManifest` в репозитории сервера; эта библиотека только читает и валидирует.
