## AIGuiders.DotnetMcp.Templates

Template pack for `dotnet new` to bootstrap MCP servers on .NET.

Release notes: see `docs/release.md`.

### Why the template looks “heavy”

The `mcp` template is intentionally aligned with our MCP repos (for example `hybrid-codebase-index`, `agent-notes-mcp`, `git-mcp`):

- **`ToolCatalog` is the source of truth**: tool names + descriptions + input schemas live in code.
- **`mcp-tools.manifest.json` is generated** from `ToolCatalog.Build()` using `tools/ExportMcpManifest`.
  - This keeps the manifest and the docs consistent with the runtime contract.
- **`docs/MCP-TOOLS.md` is generated** from the same catalog, so humans see the same descriptions the host sees.
- **Tests validate consistency**: `*.Tests` compares manifest tool names/descriptions against `ToolCatalog`.

### Why `tools/McpToolManifest` exists (instead of “just DTOs”)

In the `mcp` template we include a tiny library `tools/McpToolManifest` that defines:

- the canonical JSON shape (`schema_version`, `mcp_id`, `title`, `tools[]`)
- a reader/validator (`McpToolManifestReader`)
- a name comparer (`ToolCatalogNameComparer`)

This gives a stable, reusable contract in every generated MCP repo and matches how the rest of our MCP repos are automated.

### Why there are two readmes

- **`README-NUGET.md`** is the public package readme shown on nuget.org (install + usage only).
- **`README.md`** is the repo readme (may include maintainer notes and links to `docs/`).

### Install

```powershell
dotnet new install AIGuiders.DotnetMcp.Templates
```

### Create a new MCP server (full pattern)

```powershell
dotnet new mcp -n MyCompany.MyMcp --mcp-id my-mcp
```

### Create a minimal MCP server

```powershell
dotnet new mcp-min -n MyCompany.MyMcp --mcp-id my-mcp
```

### Uninstall

```powershell
dotnet new uninstall AIGuiders.DotnetMcp.Templates
```

