# `aid-publish`

`aid-publish` is a .NET tool that wraps `dotnet publish` with two pragmatic features:

- Mirrors the publish output into a **fixed target directory** (no-spaces friendly) so you can reliably point MCP configs or shortcuts to a stable path.
- Optionally **kills the running app** *only if* it is running from that target path (prevents file-lock publish failures).

## Install (local tool)

From a repo root:

```bash
dotnet tool install AIGuiders.DotnetTools.PublishFixedTarget
```

## Usage

```bash
dotnet aid-publish -Project path/to/app.csproj -Target D:\my-app-debug -Runtime win-x64 -Configuration Debug -SelfContained -KillRunning
```

Show help:

```bash
dotnet aid-publish -Help
```

## Options (high level)

- `-Project`: `.csproj` to publish (required)
- `-Target`: target directory to mirror into (required)
- `-Runtime`: RID (default `win-x64`)
- `-Configuration`: `Debug` or `Release` (default `Debug`)
- `-SelfContained`: publish self-contained
- `-OutDir`: publish output directory (default: `publish-{configuration}`)
- `-AppExeName`: executable/process name if it differs from project file / `<AssemblyName>` (no `.exe`)
- `-KillRunning`: kill the app if it is running from the target path
- `-UseNuGet`: pass `/p:AidUseNuGet=true` — csproj dual refs should prefer PackageReference when `$(AidUseNuGet)==true` even if sibling `.csproj` exists
- `-MsbuildProp`: repeatable `/p:Name=Value`
- `-DotnetArg`: repeatable extra args appended to `dotnet publish`
- `-RequireMirrorFile`: repeatable relative path — after mirroring **must exist as file** under `Target`. Example for **RoslynMcp** (`Microsoft.CodeAnalysis.Workspaces.MSBuild`): `BuildHost-netcore/Microsoft.CodeAnalysis.Workspaces.MSBuild.BuildHost.dll` — catches incomplete publish output before MCP fails at runtime.

