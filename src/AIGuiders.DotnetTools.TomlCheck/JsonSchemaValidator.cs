using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace AIGuiders.DotnetTools.TomlCheck;

internal sealed class JsonSchemaValidator(string schemaPath)
{
    private readonly JsonSchema _schema = JsonSchema.FromFile(schemaPath);

    public bool TryValidate(JsonNode document, out IReadOnlyList<string> errors)
    {
        var element = document.Deserialize<JsonElement>();
        var result = _schema.Evaluate(element, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
        });

        if (result.IsValid)
        {
            errors = [];
            return true;
        }

        errors = CollectErrors(result);
        return false;
    }

    private static List<string> CollectErrors(EvaluationResults result)
    {
        var list = new List<string>();
        Walk(result, list);
        return list;
    }

    private static void Walk(EvaluationResults node, List<string> errors)
    {
        if (node.Errors is { Count: > 0 })
        {
            var location = string.IsNullOrWhiteSpace(node.EvaluationPath.ToString())
                ? "$"
                : node.EvaluationPath.ToString();
            foreach (var (key, message) in node.Errors)
                errors.Add($"{location}: {key} — {message}");
        }

        if (node.Details is null)
            return;

        foreach (var child in node.Details)
            Walk(child, errors);
    }
}
