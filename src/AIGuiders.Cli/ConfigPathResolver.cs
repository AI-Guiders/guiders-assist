namespace AIGuiders.Cli;

/// <summary>Resolve <c>--config</c> / env for MCP 2.0 hosts (shared across planet repos).</summary>
public static class ConfigPathResolver
{
    public static bool IsHelp(string[] args) =>
        args.Any(static a => a is "--help" or "-h" or "/?");

    /// <summary>Returns config path from argv or environment; null when unset.</summary>
    public static string? TryResolve(string[] args, string? environmentVariable = null)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is "--config" or "--config-file" or "-c")
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException($"Missing path after {arg}.");
                return args[i + 1].Trim();
            }

            const string prefix = "--config=";
            if (arg.StartsWith(prefix, StringComparison.Ordinal))
                return arg[prefix.Length..].Trim();
        }

        if (string.IsNullOrWhiteSpace(environmentVariable))
            return null;

        var fromEnv = Environment.GetEnvironmentVariable(environmentVariable);
        return string.IsNullOrWhiteSpace(fromEnv) ? null : fromEnv.Trim();
    }
}
