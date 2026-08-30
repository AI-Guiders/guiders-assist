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
        Console.WriteLine("aig-toml-check — validate TOML with JSON Schema");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  aig-toml-check check <path...> [--schema <file.json>]");
        Console.WriteLine();
        Console.WriteLine("Each TOML may declare a schema in the header:");
        Console.WriteLine("  #:schema ../docs/schemas/example.schema.json");
    }
}
