# guiders-assist

Federation **assist** monorepo: shared MCP tooling, dotnet templates, and small CLI helpers.  
Successor to `mcp-tool-manifest`, `dotnet-mcp-templates`, and `AIGuiders.DotnetTools` (legacy repos stay until publish is verified).

## Packages

| NuGet | Source |
|-------|--------|
| `AIGuiders.McpToolManifest` | `src/McpToolManifest` |
| `AIGuiders.DotnetMcp.Templates` | `src/DotnetMcp.Templates` |
| `AIGuiders.Cli` | `src/AIGuiders.Cli` |
| `AIGuiders.DotnetTools.PublishFixedTarget` | `src/AIGuiders.DotnetTools.PublishFixedTarget` (`aid-publish`) |
| `AIGuiders.DotnetTools.TomlCheck` | `src/AIGuiders.DotnetTools.TomlCheck` (`aig-toml-check`) |
| `AIGuiders.DotnetTools.DeckEmit` | `src/AIGuiders.DotnetTools.DeckEmit` (`deck emit`) |

**Not here:** `AIGuiders.WebcamMcp.Shared` (product domain) — stays in `webcam-mcp-shared`.

## Build

```bash
dotnet build Guiders.Assist.sln -c Release
dotnet test Guiders.Assist.sln -c Release
```

## Release

Tag `v*` or `workflow_dispatch` → `release.yml` packs all ship packages with one SemVer, pushes via Trusted Publishing (`user: AIGuiders`).

See [docs/migration.md](docs/migration.md) for legacy repo mapping and TP cutover.

## License

MIT — see [LICENSE](LICENSE).
