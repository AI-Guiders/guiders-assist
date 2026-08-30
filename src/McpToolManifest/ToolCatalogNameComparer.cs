namespace McpToolManifest;

/// <summary>Сравнение имён тулов из манифеста и из рантайм-каталога.</summary>
public static class ToolCatalogNameComparer
{
    public static IReadOnlyList<string> Compare(
        IEnumerable<string> manifestToolNames,
        IEnumerable<string> catalogToolNames)
    {
        var m = new HashSet<string>(manifestToolNames.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()), StringComparer.Ordinal);
        var c = new HashSet<string>(catalogToolNames.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()), StringComparer.Ordinal);

        var errors = new List<string>();
        foreach (var x in m)
        {
            if (!c.Contains(x))
                errors.Add($"In manifest but not in catalog: {x}");
        }

        foreach (var x in c)
        {
            if (!m.Contains(x))
                errors.Add($"In catalog but not in manifest: {x}");
        }

        return errors;
    }
}
