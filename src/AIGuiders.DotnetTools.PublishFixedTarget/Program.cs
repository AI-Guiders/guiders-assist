using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Ookii.CommandLine;

namespace AIGuiders.DotnetTools.PublishFixedTarget;

[GeneratedParser]
[ApplicationFriendlyName("aid-publish")]
[Description("dotnet publish wrapper: mirror to fixed target, optional kill-locking process, and proof timestamps.")]
partial class PublishArgs
{
    [CommandLineArgument]
    [Description("Path to the .csproj to publish.")]
    public required string Project { get; set; }

    [CommandLineArgument]
    [Description("Target directory to mirror publish output into.")]
    public required string Target { get; set; }

    [CommandLineArgument]
    [Description("Runtime identifier (RID).")]
    public string Runtime { get; set; } = "win-x64";

    [CommandLineArgument]
    [Description("Configuration: Debug or Release.")]
    public string Configuration { get; set; } = "Debug";

    [CommandLineArgument]
    [Description("Publish as self-contained.")]
    public bool SelfContained { get; set; }

    [CommandLineArgument]
    [Description("Publish output directory. Default: publish-{configuration-lower}.")]
    public string? OutDir { get; set; }

    [CommandLineArgument]
    [Description("Executable/process name if it differs from project file / AssemblyName (no .exe).")]
    public string? AppExeName { get; set; }

    [CommandLineArgument]
    [Description("Kill the app if it is running from the target path (avoids file locks).")]
    public bool KillRunning { get; set; }

    [CommandLineArgument]
    [Description("Prefer NuGet PackageReference over sibling ProjectReference: passes /p:AidUseNuGet=true. Projects must condition dual refs on $(AidUseNuGet).")]
    public bool UseNuGet { get; set; }

    [CommandLineArgument]
    [Description("Repeatable MSBuild property arguments (e.g. /p:GenerateIdeProtocolDocs=false).")]
    public string[]? MsbuildProp { get; set; }

    [CommandLineArgument]
    [Description("Repeatable extra arguments appended to dotnet publish.")]
    public string[]? DotnetArg { get; set; }

    [CommandLineArgument]
    [Description("After mirror: each path must exist as a file under Target (relative; / or \\\\ ok). Typical: roslyn MCP + MSBuild Workspace — BuildHost-netcore/Microsoft.CodeAnalysis.Workspaces.MSBuild.BuildHost.dll.")]
    public string[]? RequireMirrorFile { get; set; }
}

internal static class Program
{
    public static int Main(string[] args)
    {
        var a = PublishArgs.Parse(args);
        if (a is null)
            return 1;

        try
        {
            var projectPath = Path.GetFullPath(a.Project);
            if (!File.Exists(projectPath))
                throw new FileNotFoundException("Project not found.", projectPath);

            var projectDir = Path.GetDirectoryName(projectPath)!;
            var configuration = NormalizeConfiguration(a.Configuration);

            var outDir = string.IsNullOrWhiteSpace(a.OutDir)
                ? Path.Combine(projectDir, $"publish-{configuration.ToLowerInvariant()}")
                : Path.GetFullPath(Path.IsPathRooted(a.OutDir) ? a.OutDir : Path.Combine(projectDir, a.OutDir));

            var targetDir = Path.GetFullPath(a.Target);
            Directory.CreateDirectory(outDir);
            Directory.CreateDirectory(targetDir);

            var exeName = ResolveExeName(projectPath, a.AppExeName);
            var expectedTargetExe = Path.Combine(targetDir, $"{exeName}.exe");
            StopIfRunningFromTarget(exeName, expectedTargetExe, a.KillRunning);

            RunDotnetPublish(projectPath, outDir, a.Runtime, configuration, a.SelfContained, a.UseNuGet, a.MsbuildProp, a.DotnetArg);

            MirrorDirectory(outDir, targetDir);

            RequireMirrorArtifacts(targetDir, a.RequireMirrorFile);

            var publishExe = Path.Combine(outDir, $"{exeName}.exe");
            var targetExe = expectedTargetExe;

            if (File.Exists(publishExe) && File.Exists(targetExe))
            {
                var publishTs = File.GetLastWriteTimeUtc(publishExe);
                var targetTs = File.GetLastWriteTimeUtc(targetExe);
                Console.WriteLine();
                Console.WriteLine($"OK: {targetExe}  (UTC {targetTs:O})");
                Console.WriteLine($"     publish: {publishTs:O}");
                Console.WriteLine($"     target:  {targetTs:O}");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine($"OK: mirrored to {targetDir}");
                Console.WriteLine();
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>
    /// Process/exe name: -AppExeName → csproj AssemblyName → project file name.
    /// (Anui.Agent.Mcp.csproj publishes AnuiAgentMcp.exe — project name alone breaks -KillRunning.)
    /// </summary>
    private static string ResolveExeName(string projectPath, string? appExeName)
    {
        if (!string.IsNullOrWhiteSpace(appExeName))
        {
            var n = appExeName.Trim();
            if (n.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                n = n[..^4];
            return n;
        }

        try
        {
            var doc = XDocument.Load(projectPath);
            var asm = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "AssemblyName")
                ?.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(asm))
                return asm!;
        }
        catch
        {
            // fall through
        }

        return Path.GetFileNameWithoutExtension(projectPath);
    }

    private static string NormalizeConfiguration(string configuration)
    {
        var c = configuration.Trim();
        return string.Equals(c, "Release", StringComparison.OrdinalIgnoreCase) ? "Release" : "Debug";
    }

    private static void RunDotnetPublish(
        string projectPath,
        string outDir,
        string runtime,
        string configuration,
        bool selfContained,
        bool useNuGet,
        string[]? msbuildProps,
        string[]? extraArgs)
    {
        var args = new List<string>
        {
            "publish",
            QuoteIfNeeded(projectPath),
            "-c", configuration,
            "-r", runtime,
            "-o", QuoteIfNeeded(outDir),
            "-v", "minimal",
        };

        if (selfContained)
        {
            args.Add("--self-contained");
            args.Add("true");
        }

        if (useNuGet)
            args.Add("/p:AidUseNuGet=true");

        if (msbuildProps is { Length: > 0 })
            args.AddRange(msbuildProps.Where(p => !string.IsNullOrWhiteSpace(p)));

        if (extraArgs is { Length: > 0 })
            args.AddRange(extraArgs.Where(p => !string.IsNullOrWhiteSpace(p)));

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = string.Join(' ', args),
            UseShellExecute = false,
        };

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet.");
        p.WaitForExit();
        if (p.ExitCode != 0)
            throw new InvalidOperationException($"dotnet publish failed with exit code {p.ExitCode}.");
    }

    private static void StopIfRunningFromTarget(string processName, string expectedExePath, bool killRunning)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        var expected = Path.GetFullPath(expectedExePath);
        var procs = Process.GetProcessesByName(processName);
        foreach (var p in procs)
        {
            string? path = null;
            try { path = p.MainModule?.FileName; } catch { path = null; }
            if (string.IsNullOrWhiteSpace(path))
                continue;

            if (!string.Equals(Path.GetFullPath(path), expected, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!killRunning)
                throw new InvalidOperationException($"{processName} is running from target path and will lock publish output. Close it or re-run with --kill-running.");

            try
            {
                Console.WriteLine($"Stopping {processName} PID {p.Id} from {path}");
                p.Kill(entireProcessTree: true);
                p.WaitForExit(10_000);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to stop {processName} (PID {p.Id}): {ex.Message}");
            }
        }
    }

    private static void MirrorDirectory(string sourceDir, string targetDir)
    {
        sourceDir = Path.GetFullPath(sourceDir);
        targetDir = Path.GetFullPath(targetDir);

        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException(sourceDir);

        Directory.CreateDirectory(targetDir);

        foreach (var srcPath in Directory.EnumerateFileSystemEntries(sourceDir, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(sourceDir, srcPath);
            var dstPath = Path.Combine(targetDir, rel);

            if (Directory.Exists(srcPath))
            {
                Directory.CreateDirectory(dstPath);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(dstPath)!);
            File.Copy(srcPath, dstPath, overwrite: true);

            try
            {
                File.SetLastWriteTimeUtc(dstPath, File.GetLastWriteTimeUtc(srcPath));
            }
            catch
            {
                // ignore
            }
        }

        foreach (var dstPath in Directory.EnumerateFileSystemEntries(targetDir, "*", SearchOption.AllDirectories)
                     .OrderByDescending(p => p.Length))
        {
            var rel = Path.GetRelativePath(targetDir, dstPath);
            var srcPath = Path.Combine(sourceDir, rel);
            if (File.Exists(dstPath))
            {
                if (!File.Exists(srcPath))
                    File.Delete(dstPath);
            }
            else if (Directory.Exists(dstPath))
            {
                if (!Directory.Exists(srcPath) && IsDirectoryEmpty(dstPath))
                    Directory.Delete(dstPath, recursive: true);
            }
        }
    }

    private static bool IsDirectoryEmpty(string path) =>
        !Directory.EnumerateFileSystemEntries(path).Any();

    private static void RequireMirrorArtifacts(string targetDir, string[]? relativeFilePaths)
    {
        if (relativeFilePaths is not { Length: > 0 })
            return;

        var targetRoot = Path.GetFullPath(targetDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var boundary = targetRoot + Path.DirectorySeparatorChar;

        foreach (var raw in relativeFilePaths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            var rel = NormalizeRelativeRequirement(raw.Trim());
            var resolved = Path.GetFullPath(Path.Combine(targetRoot, rel));
            if (!resolved.StartsWith(boundary, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(resolved, targetRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"RequireMirrorFile must stay under Target: `{raw}`");
            }

            if (!File.Exists(resolved))
            {
                throw new FileNotFoundException(
                    $"RequireMirrorFile not found after mirror (incomplete dotnet publish?): {rel}",
                    resolved);
            }
        }
    }

    private static string NormalizeRelativeRequirement(string raw) =>
        raw.Replace('/', Path.DirectorySeparatorChar).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Trim('\\');

    private static string QuoteIfNeeded(string s) =>
        s.Contains(' ') ? $"\"{s}\"" : s;
}
