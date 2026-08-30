## Release (nuget.org)

This repository uses **NuGet Trusted Publishing (OIDC)** — no long-lived API keys.

### One-time setup (nuget.org)

On `nuget.org`: **Trusted publishing** → add policy for GitHub Actions:
- **Repository owner**: `KarataevDmitry`
- **Repository**: `dotnet-mcp-templates`
- **Workflow file**: `publish.yml`
- **Environment**: (leave empty)

### Publish a version

Create and push a tag like `v0.1.10` — GitHub Actions workflow `publish.yml` will pack and push the package.

