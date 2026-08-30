namespace AIGuiders.DotnetTools.TomlCheck;

internal static class CheckArgsParser
{
    public static bool TryParse(string[] args, out string[] paths, out string? schema, out string? error)
    {
        paths = [];
        schema = null;
        error = null;

        if (args.Length == 0)
        {
            error = "check: at least one path is required.";
            return false;
        }

        var pathList = new List<string>();
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is "--schema" or "-schema")
            {
                if (i + 1 >= args.Length)
                {
                    error = "check: --schema requires a path.";
                    return false;
                }

                schema = args[++i];
                continue;
            }

            if (arg.StartsWith('-'))
            {
                error = $"check: unknown option: {arg}";
                return false;
            }

            pathList.Add(arg);
        }

        if (pathList.Count == 0)
        {
            error = "check: at least one path is required.";
            return false;
        }

        paths = pathList.ToArray();
        return true;
    }
}
