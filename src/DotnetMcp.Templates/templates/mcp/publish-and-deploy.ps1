# Publish Release (win-x64, self-contained) and mirror to a fixed path for Cursor MCP.
# Run from repo:  cd ...\AIGuiders.SampleMcp  ;  .\publish-and-deploy.ps1
# Optional: -Target "D:\sample-mcp"
[CmdletBinding()]
param(
    [string] $Target = "D:\sample-mcp"
)

$ErrorActionPreference = "Stop"
$here = $PSScriptRoot
$csproj = Join-Path $here "AIGuiders.SampleMcp.csproj"
if (-not (Test-Path -LiteralPath $csproj)) {
    Write-Error "AIGuiders.SampleMcp.csproj not found. Run this script from the MCP directory (PSScriptRoot=$here)."
    exit 1
}

Push-Location $here
try {
    # Keep docs/manifests in sync with ToolCatalog.
    & dotnet run --project (Join-Path $here "tools\\ExportMcpManifest\\ExportMcpManifest.csproj") -- --write | Out-Null
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $outDir = Join-Path $here "publish"
    $publishArgs = @(
        "publish", $csproj,
        "-c", "Release",
        "-r", "win-x64",
        "-o", $outDir,
        "-v", "minimal"
    )

    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    if (-not (Test-Path -LiteralPath $Target)) {
        New-Item -ItemType Directory -Path $Target -Force | Out-Null
    }

    robocopy $outDir $Target /E /MIR /NFL /NDL /NJH /NJS /R:2 /W:1 | Out-Null
    $robocode = $LASTEXITCODE
    if ($robocode -ge 8) {
        Write-Error "robocopy failed with exit code $robocode"
        exit $robocode
    }

    $exe = Join-Path $Target "AIGuiders.SampleMcp.exe"
    if (-not (Test-Path -LiteralPath $exe)) {
        Write-Error "Expected exe not found: $exe"
        exit 1
    }

    $ts = (Get-Item -LiteralPath $exe).LastWriteTimeUtc.ToString("o")
    $exeJson = $exe.Replace('\', '\\')
    Write-Host ""
    Write-Host "OK: $exe  (UTC $ts)"
    Write-Host ""
    Write-Host "Cursor MCP: paste into mcp.json ->"
    Write-Host @"
  "sample-mcp": {
    "command": "$exeJson",
    "args": []
  }
"@
    Write-Host ""
} finally {
    Pop-Location
}

