using System.Text.Json;
using System.Text.Json.Nodes;

namespace RabbitMqEventConsumer;

public class JsonReplacementService
{
    private readonly JsonReplacementConfig _config;

    public JsonReplacementService(JsonReplacementConfig config)
    {
        _config = config;
    }

    public (string processedMessage, List<string> appliedRules) ProcessMessage(string originalMessage, bool isJson)
    {
        var appliedRules = new List<string>();
        
        if (!_config.EnableReplacements || !isJson || !_config.Rules.Any(r => r.Enabled))
        {
            return (originalMessage, appliedRules);
        }

        try
        {
            var jsonNode = JsonNode.Parse(originalMessage);
            if (jsonNode == null) return (originalMessage, appliedRules);

            foreach (var rule in _config.Rules.Where(r => r.Enabled))
            {
                if (ApplyReplacementRule(jsonNode, rule))
                {
                    appliedRules.Add($"{rule.JsonPath} → {rule.Placeholder}");
                }
            }

            var processedMessage = jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            return (processedMessage, appliedRules);
        }
        catch (Exception)
        {
            // If JSON processing fails, return original message
            return (originalMessage, appliedRules);
        }
    }

    private bool ApplyReplacementRule(JsonNode jsonNode, JsonReplacementRule rule)
    {
        try
        {
            var pathParts = ParseJsonPath(rule.JsonPath);
            return ReplaceValueAtPath(jsonNode, pathParts, rule.Placeholder);
        }
        catch (Exception)
        {
            // Skip this rule if path is invalid
            return false;
        }
    }

    private string[] ParseJsonPath(string jsonPath)
    {
        // Simple JSONPath parser - handles dot notation like "user.profile.email"
        // and array notation like "users[0].name"
        return jsonPath.Split('.')
            .SelectMany(part => 
            {
                if (part.Contains('[') && part.Contains(']'))
                {
                    // Handle array notation like "users[0]"
                    var beforeBracket = part.Substring(0, part.IndexOf('['));
                    var insideBrackets = part.Substring(part.IndexOf('[') + 1, part.IndexOf(']') - part.IndexOf('[') - 1);
                    return new[] { beforeBracket, $"[{insideBrackets}]" };
                }
                return new[] { part };
            })
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();
    }

    private bool ReplaceValueAtPath(JsonNode node, string[] pathParts, string placeholder)
    {
        if (pathParts.Length == 0 || node == null) return false;

        if (pathParts.Length == 1)
        {
            // Last part of path - replace the value
            var key = pathParts[0];
            
            if (key.StartsWith('[') && key.EndsWith(']'))
            {
                // Array index
                var indexStr = key.Substring(1, key.Length - 2);
                if (int.TryParse(indexStr, out var index) && node is JsonArray array && index >= 0 && index < array.Count)
                {
                    array[index] = JsonValue.Create(placeholder);
                    return true;
                }
            }
            else if (node is JsonObject obj && obj.ContainsKey(key))
            {
                // Object property
                obj[key] = JsonValue.Create(placeholder);
                return true;
            }
        }
        else
        {
            // Navigate deeper into the structure
            var currentKey = pathParts[0];
            var remainingPath = pathParts.Skip(1).ToArray();

            if (currentKey.StartsWith('[') && currentKey.EndsWith(']'))
            {
                // Array index
                var indexStr = currentKey.Substring(1, currentKey.Length - 2);
                if (int.TryParse(indexStr, out var index) && node is JsonArray array && index >= 0 && index < array.Count)
                {
                    return ReplaceValueAtPath(array[index]!, remainingPath, placeholder);
                }
            }
            else if (node is JsonObject obj && obj.ContainsKey(currentKey))
            {
                // Object property
                return ReplaceValueAtPath(obj[currentKey]!, remainingPath, placeholder);
            }
        }
        
        return false;
    }

    public static List<string> ExtractJsonPaths(string jsonMessage)
    {
        var paths = new List<string>();
        
        try
        {
            var jsonNode = JsonNode.Parse(jsonMessage);
            if (jsonNode != null)
            {
                ExtractPathsRecursive(jsonNode, "", paths);
            }
        }
        catch (Exception)
        {
            // If parsing fails, return empty list
        }

        return paths.OrderBy(p => p).ToList();
    }

    private static void ExtractPathsRecursive(JsonNode node, string currentPath, List<string> paths)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var kvp in obj)
                {
                    var newPath = string.IsNullOrEmpty(currentPath) ? kvp.Key : $"{currentPath}.{kvp.Key}";
                    paths.Add(newPath);
                    if (kvp.Value != null)
                    {
                        ExtractPathsRecursive(kvp.Value, newPath, paths);
                    }
                }
                break;
                
            case JsonArray array:
                for (int i = 0; i < array.Count; i++)
                {
                    var newPath = $"{currentPath}[{i}]";
                    paths.Add(newPath);
                    if (array[i] != null)
                    {
                        ExtractPathsRecursive(array[i]!, newPath, paths);
                    }
                }
                break;
        }
    }
}
