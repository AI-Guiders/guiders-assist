using System.Text.Json.Nodes;
using Tomlyn.Model;

namespace AIGuiders.DotnetTools.TomlCheck;

internal static class TomlToJsonConverter
{
    public static JsonNode ToJsonNode(TomlTable table) => ConvertTable(table);

    private static JsonObject ConvertTable(TomlTable table)
    {
        var obj = new JsonObject();
        foreach (var (key, value) in table)
            obj[key] = ConvertValue(value);
        return obj;
    }

    private static JsonArray ConvertTableArray(TomlTableArray array)
    {
        var arr = new JsonArray();
        foreach (var table in array)
            arr.Add(ConvertTable(table));
        return arr;
    }

    private static JsonArray ConvertArray(TomlArray array)
    {
        var arr = new JsonArray();
        foreach (var item in array)
            arr.Add(ConvertValue(item));
        return arr;
    }

    private static JsonNode? ConvertValue(object? value) =>
        value switch
        {
            null => null,
            TomlTable t => ConvertTable(t),
            TomlTableArray ta => ConvertTableArray(ta),
            TomlArray a => ConvertArray(a),
            string s => JsonValue.Create(s),
            bool b => JsonValue.Create(b),
            long l => JsonValue.Create(l),
            int i => JsonValue.Create(i),
            double d => JsonValue.Create(d),
            float f => JsonValue.Create(f),
            DateTimeOffset dto => JsonValue.Create(dto.ToString("O")),
            DateTime dt => JsonValue.Create(dt.ToString("O")),
            _ => JsonValue.Create(value.ToString()),
        };
}
