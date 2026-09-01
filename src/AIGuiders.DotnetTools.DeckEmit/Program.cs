using AIGuiders.Platform.Authoring.Deck;
using AIGuiders.Surface.Wpf.CodeGen;

if (args.Length == 0 || args is ["--help"] or ["-h"] or ["help"])
{
    PrintUsage();
    return 0;
}

return args[0] switch
{
    "emit" => EmitCommand.Run(args.Skip(1).ToArray()),
    "--version" or "-v" => PrintVersion(),
    _ => Unknown(args[0]),
};

static int PrintVersion()
{
    Console.WriteLine("deck 0.1.0");
    return 0;
}

static int PrintUsage()
{
    Console.WriteLine(
        """
        deck — federation .deck toolchain

        Usage:
          deck emit <file.deck> [--namespace N] [--class C] [--output path]
          deck --version
          deck --help
        """);
    return 0;
}

static int Unknown(string command)
{
    Console.Error.WriteLine($"Unknown command: {command}");
    PrintUsage();
    return 2;
}

internal static class EmitCommand
{
    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0].StartsWith('-'))
        {
            Console.Error.WriteLine("emit: missing file path");
            return 2;
        }

        var path = args[0];
        var namespaceName = "Generated.Deck";
        var className = "DeckIds";
        string? outputPath = null;

        for (var i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--namespace" when i + 1 < args.Length:
                    namespaceName = args[++i];
                    break;
                case "--class" when i + 1 < args.Length:
                    className = args[++i];
                    break;
                case "--output" when i + 1 < args.Length:
                    outputPath = args[++i];
                    break;
                default:
                    Console.Error.WriteLine($"emit: unknown argument `{args[i]}`");
                    return 2;
            }
        }

        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"emit: file not found: {path}");
            return 1;
        }

        var result = DeckParser.ParseFile(path);
        if (result.Document is null)
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                Console.Error.WriteLine($"{path}:{diagnostic.Line}: {diagnostic.Message}");
            }

            return 1;
        }

        var code = DeckIdsEmitter.EmitCSharp(result.Document, namespaceName, className);
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            Console.WriteLine(code);
            return 0;
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, code);
        return 0;
    }
}
