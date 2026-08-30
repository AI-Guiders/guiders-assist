# Migration from legacy assist repos

| Legacy repo | Status | NuGet IDs |
|-------------|--------|-----------|
| [mcp-tool-manifest](https://github.com/AI-Guiders/mcp-tool-manifest) | **superseded** — do not publish | `AIGuiders.McpToolManifest` |
| [dotnet-mcp-templates](https://github.com/AI-Guiders/dotnet-mcp-templates) | **superseded** | `AIGuiders.DotnetMcp.Templates` |
| [AIGuiders.DotnetTools](https://github.com/AI-Guiders/AIGuiders.DotnetTools) | **superseded** | `AIGuiders.Cli`, `AIGuiders.DotnetTools.*` |

## Cutover checklist

1. Create TP on nuget.org: **Account → Trusted Publishing**, Package owner `AIGuiders`, repo `guiders-assist`, workflow `release.yml`, globs for assist package IDs.
2. Smoke: `workflow_dispatch` release with patch version.
3. Archive legacy repos (README pointer only); delete legacy TP policies.
4. Update consumer repo URLs if needed (package IDs unchanged).

## Versioning

Monorepo uses **one SemVer per tag** (`v1.2.3` → all ship packages `1.2.3`). `skip-duplicate` on push tolerates unchanged package bodies.
