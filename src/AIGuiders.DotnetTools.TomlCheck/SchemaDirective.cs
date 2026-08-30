using System.Text.RegularExpressions;

namespace AIGuiders.DotnetTools.TomlCheck;

internal static partial class SchemaDirective
{
    // Taplo-style: #:schema relative/or/absolute/path.json
    [GeneratedRegex(@"^\s*#\s*:\s*schema\s+(\S+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SchemaLineRegex();

    /// <summary>Reads the first 32 lines for a #:schema directive.</summary>
    public static string? TryReadSchemaPath(string tomlPath)
    {
        using var reader = File.OpenText(tomlPath);
        for (var i = 0; i < 32 && !reader.EndOfStream; i++)
        {
            var line = reader.ReadLine();
            if (line is null)
                break;

            var match = SchemaLineRegex().Match(line);
            if (match.Success)
                return match.Groups[1].Value.Trim().Trim('"', '\'');
        }

        return null;
    }

    public static string ResolveSchemaPath(string tomlPath, string schemaRef)
    {
        if (Path.IsPathRooted(schemaRef))
            return Path.GetFullPath(schemaRef);

        var tomlDir = Path.GetDirectoryName(Path.GetFullPath(tomlPath))
            ?? Environment.CurrentDirectory;
        return Path.GetFullPath(Path.Combine(tomlDir, schemaRef));
    }
}
