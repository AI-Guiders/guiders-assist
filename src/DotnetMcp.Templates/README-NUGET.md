## AIGuiders.DotnetMcp.Templates

Template pack for `dotnet new` to bootstrap MCP servers on .NET.

### Why

Use this template pack when you want to start a **.NET MCP server** without rebuilding the same boilerplate every time (host wiring, tool registration skeleton, publish/deploy scripts, and manifest/docs conventions).

It’s especially useful if you maintain multiple MCP repos and want a consistent “shape” across them.

### When to use

- You’re creating a new MCP server for Cursor / Claude Desktop / your own stdio host.
- You want a repo scaffold that can grow from POC to “real project” without a rewrite.
- You want a stable convention for tool contracts and automation (manifest/docs generation).

### Templates

- `mcp`: full scaffold (recommended for real projects).
- `mcp-min`: minimal scaffold (good for small experiments).

**Common structure (both templates):**

- `Program.cs`: server bootstrap (stdio transport).
- `ToolCatalog.cs`: tool definitions (names + descriptions + schemas) in one place.
- `ToolHandlers.cs`: handler methods that implement the tools.

**`mcp-min` includes:**

- The minimal runnable server + sample tool wiring.
- No extra automation/testing layers.

**`mcp` (full) additionally includes:**

- **Manifest/docs automation**: generate `mcp-tools.manifest.json` and `docs/MCP-TOOLS.md` from `ToolCatalog`.
- **Consistency tests**: validate that docs/manifest match the runtime tool catalog.
- Small shared helpers/libraries used by the automation (so each generated repo doesn’t re-invent the JSON contract).

### What you get (high level)

The generated project gives you a working starting point with a basic structure for tools/handlers.  
The full template is aligned with a “tool catalog as source of truth” workflow (manifest/docs automation).

### Non-goals

- This package is not an MCP framework/runtime library — it’s a **scaffold**.
- It does not decide auth/permissions/policy (that’s host- and deployment-specific).
- It doesn’t design your domain tools for you — it gives rails, not product semantics.

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

