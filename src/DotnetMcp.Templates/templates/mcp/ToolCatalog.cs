using System.Text.Json;
using ModelContextProtocol.Protocol;
using Tool = ModelContextProtocol.Protocol.Tool;

namespace AIGuiders.SampleMcp;

internal static class ToolCatalog
{
    private static JsonElement Schema(object schema) => JsonSerializer.SerializeToElement(schema);

    internal static List<Tool> Build() =>
    [
        new()
        {
            Name = "hello",
            Description = "Example tool: returns a greeting string.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    name = new { type = "string", description = "Optional name." }
                }
            })
        }
    ];
}

