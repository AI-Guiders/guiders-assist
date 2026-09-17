namespace TomlSchema.Tests;

using Xunit;

public class TestBase
{
    protected static string FixtureRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            var marker = Path.Combine(dir, "third-party", "toml-schema", "toml-schema.abnf");
            if (File.Exists(marker))
                return Path.GetDirectoryName(marker)!;

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException("Cannot find third-party/toml-schema fixtures.");
    }

    protected static string Fixture(string relativePath) =>
        Path.Combine(FixtureRoot(), relativePath);

    protected static string Write(string fileName, string content)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        File.WriteAllText(tempPath, content);
        return tempPath;
    }
}
