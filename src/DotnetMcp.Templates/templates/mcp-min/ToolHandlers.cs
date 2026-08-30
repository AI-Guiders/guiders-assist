using System.Collections.Frozen;
using System.Text.Json;

namespace AIGuiders.SampleMcp;

internal static class ToolHandlers
{
    internal static string Handle(string name, IReadOnlyDictionary<string, JsonElement> args)
    {
        args ??= FrozenDictionary<string, JsonElement>.Empty;
        return name switch
        {
            "hello" => HandleHello(args),
            _ => throw new ArgumentException($"Unknown tool: {name}", nameof(name)),
        };
    }

    private static string HandleHello(IReadOnlyDictionary<string, JsonElement> args)
    {
        var who = args.TryGetValue("name", out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;

        who = string.IsNullOrWhiteSpace(who) ? "world" : who.Trim();
        return $"Hello, {who}!";
    }
}

