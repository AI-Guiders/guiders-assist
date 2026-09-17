namespace AIGuiders.DotnetTools.TomlCheck;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args))
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        if (!string.Equals(args[0], "check", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"Unknown command: {args[0]}");
            PrintUsage();
            return 1;
        }

        if (!CheckArgsParser.TryParse(args[1..], out var paths, out var schema, out var error))
        {
            Console.Error.WriteLine(error);
            return 1;
        }

        var engine = new TomlCheckEngine(schema);
        return engine.CheckPaths(paths);
    }

    private static bool IsHelp(string[] args) =>
        args.Any(a => string.Equals(a, "-?", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(a, "-Help", StringComparison.OrdinalIgnoreCase));

    private static void PrintUsage()
    {
        Console.WriteLine("aig-toml-check — validate TOML with TOML Schema (.tosd) or JSON Schema");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  aig-toml-check check <path...> [--schema <file.tosd|file.json>]");
        Console.WriteLine();
        Console.WriteLine("Schema resolution (first match wins):");
        Console.WriteLine("  1. --schema override");
        Console.WriteLine("  2. #:schema directive in the first 32 lines");
        Console.WriteLine("  3. [toml-schema].location in the document");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  #:schema config.tosd");
        Console.WriteLine("  #:schema catalog.schema.json");
    }
}
