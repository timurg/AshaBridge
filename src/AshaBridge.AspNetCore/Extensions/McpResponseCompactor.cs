using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace AshaBridge.AspNetCore.Extensions;

internal static class McpResponseCompactor
{
    private const int MinimumResponseChars = 256;
    private const int MaximumResponseChars = 100_000;
    private const int MaximumStringChars = 512;

    public static void Compact(
        CallToolResult result,
        HttpContext? http,
        string? toolName,
        ILoggerFactory? loggerFactory)
    {
        var options = CompactionOptions.FromQuery(http?.Request.Query);
        if (!options.Enabled || result.IsError is true)
        {
            return;
        }

        var originalChars = 0;
        var compactedChars = 0;
        foreach (var block in result.Content)
        {
            if (block is not TextContentBlock textBlock)
            {
                continue;
            }

            originalChars += textBlock.Text.Length;
            textBlock.Text = CompactText(textBlock.Text, options);
            compactedChars += textBlock.Text.Length;
        }

        if (result.StructuredContent is JsonElement structuredContent)
        {
            var structuredText = structuredContent.GetRawText();
            originalChars += structuredText.Length;
            var compactedStructuredText = CompactText(structuredText, options);
            result.StructuredContent = JsonSerializer.Deserialize<JsonElement>(compactedStructuredText);
            compactedChars += compactedStructuredText.Length;
        }

        if (originalChars != compactedChars)
        {
            loggerFactory?.CreateLogger("AshaBridge.McpResponseCompactor").LogInformation(
                "Compacted MCP tool response. ToolName={ToolName}; OriginalChars={OriginalChars}; CompactedChars={CompactedChars}; DropZero={DropZero}; MaxResponseChars={MaxResponseChars}",
                toolName,
                originalChars,
                compactedChars,
                options.DropZero,
                options.MaxResponseChars);
        }
    }

    internal static string CompactText(string text, bool dropZero, int? maxResponseChars)
    {
        var options = new CompactionOptions(true, dropZero, NormalizeLimit(maxResponseChars));
        return CompactText(text, options);
    }

    private static string CompactText(string text, CompactionOptions options)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(text);
        }
        catch (JsonException)
        {
            return TruncatePlainText(text, options.MaxResponseChars);
        }

        if (root is null)
        {
            return text;
        }

        Prune(root, options.DropZero);
        var compacted = root.ToJsonString();
        if (options.MaxResponseChars is not int limit || compacted.Length <= limit)
        {
            return compacted;
        }

        var originalChars = compacted.Length;
        TruncateLongStrings(root);
        var target = Math.Max(MinimumResponseChars, limit - 140);
        compacted = root.ToJsonString();
        while (compacted.Length > target && RemoveLargestOptionalValue(root))
        {
            compacted = root.ToJsonString();
        }

        if (root is JsonObject obj)
        {
            obj["_ashabridge"] = new JsonObject
            {
                ["truncated"] = true,
                ["originalChars"] = originalChars,
                ["maxChars"] = limit
            };
            compacted = obj.ToJsonString();
        }

        if (compacted.Length <= limit)
        {
            return compacted;
        }

        return new JsonObject
        {
            ["_ashabridge"] = new JsonObject
            {
                ["truncated"] = true,
                ["originalChars"] = originalChars,
                ["maxChars"] = limit,
                ["message"] = "Response omitted because it could not be compacted safely."
            }
        }.ToJsonString();
    }

    private static void Prune(JsonNode node, bool dropZero)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Select(property => property.Key).ToArray())
                {
                    var child = obj[key];
                    if (child is not null)
                    {
                        Prune(child, dropZero);
                    }

                    if (IsEmpty(child) || dropZero && IsNumericZero(child))
                    {
                        obj.Remove(key);
                    }
                }

                break;
            case JsonArray array:
                for (var index = array.Count - 1; index >= 0; index--)
                {
                    var child = array[index];
                    if (child is not null)
                    {
                        Prune(child, dropZero);
                    }

                    if (IsEmpty(child) || dropZero && IsNumericZero(child))
                    {
                        array.RemoveAt(index);
                    }
                }

                break;
        }
    }

    private static bool IsEmpty(JsonNode? node) => node switch
    {
        null => true,
        JsonObject obj => obj.Count == 0,
        JsonArray array => array.Count == 0,
        JsonValue value when value.TryGetValue<string>(out var text) => string.IsNullOrWhiteSpace(text),
        _ => false
    };

    private static bool IsNumericZero(JsonNode? node) => node is JsonValue value &&
        (value.TryGetValue<int>(out var intValue) && intValue == 0
         || value.TryGetValue<long>(out var longValue) && longValue == 0
         || value.TryGetValue<decimal>(out var decimalValue) && decimalValue == 0
         || value.TryGetValue<double>(out var doubleValue) && doubleValue == 0);

    private static void TruncateLongStrings(JsonNode node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Select(property => property.Key).ToArray())
                {
                    if (obj[key] is JsonValue value && value.TryGetValue<string>(out var text) && text.Length > MaximumStringChars)
                    {
                        obj[key] = string.Concat(text.AsSpan(0, MaximumStringChars), "...[truncated]");
                    }
                    else if (obj[key] is JsonNode child)
                    {
                        TruncateLongStrings(child);
                    }
                }

                break;
            case JsonArray array:
                for (var index = 0; index < array.Count; index++)
                {
                    if (array[index] is JsonValue value && value.TryGetValue<string>(out var text) && text.Length > MaximumStringChars)
                    {
                        array[index] = string.Concat(text.AsSpan(0, MaximumStringChars), "...[truncated]");
                    }
                    else if (array[index] is JsonNode child)
                    {
                        TruncateLongStrings(child);
                    }
                }

                break;
        }
    }

    private static bool RemoveLargestOptionalValue(JsonNode root)
    {
        RemovalCandidate? largest = null;
        FindRemovalCandidate(root, depth: 0, ref largest);
        if (largest is null)
        {
            return false;
        }

        largest.Remove();
        return true;
    }

    private static void FindRemovalCandidate(JsonNode node, int depth, ref RemovalCandidate? largest)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var (key, child) in obj)
                {
                    if (child is null)
                    {
                        continue;
                    }

                    if (depth > 0 && !IsProtectedKey(key))
                    {
                        Consider(new RemovalCandidate(child.ToJsonString().Length, () => obj.Remove(key)), ref largest);
                    }

                    FindRemovalCandidate(child, depth + 1, ref largest);
                }

                break;
            case JsonArray array:
                for (var index = 0; index < array.Count; index++)
                {
                    var child = array[index];
                    if (child is null)
                    {
                        continue;
                    }

                    var capturedIndex = index;
                    if (depth > 0)
                    {
                        Consider(new RemovalCandidate(child.ToJsonString().Length, () => array.RemoveAt(capturedIndex)), ref largest);
                    }

                    FindRemovalCandidate(child, depth + 1, ref largest);
                }

                break;
        }
    }

    private static void Consider(RemovalCandidate candidate, ref RemovalCandidate? largest)
    {
        if (largest is null || candidate.Size > largest.Size)
        {
            largest = candidate;
        }
    }

    private static bool IsProtectedKey(string key) => key.Equals("id", StringComparison.OrdinalIgnoreCase)
        || key.Equals("title", StringComparison.OrdinalIgnoreCase)
        || key.Equals("name", StringComparison.OrdinalIgnoreCase)
        || key.Equals("error", StringComparison.OrdinalIgnoreCase)
        || key.Equals("message", StringComparison.OrdinalIgnoreCase);

    private static string TruncatePlainText(string text, int? maxResponseChars)
    {
        if (maxResponseChars is not int limit || text.Length <= limit)
        {
            return text;
        }

        const string suffix = "...[truncated]";
        return string.Concat(text.AsSpan(0, Math.Max(0, limit - suffix.Length)), suffix);
    }

    private static int? NormalizeLimit(int? value) => value is null
        ? null
        : Math.Clamp(value.Value, MinimumResponseChars, MaximumResponseChars);

    private sealed record RemovalCandidate(int Size, Action Remove);

    private sealed record CompactionOptions(bool Enabled, bool DropZero, int? MaxResponseChars)
    {
        public static CompactionOptions FromQuery(IQueryCollection? query)
        {
            var compact = GetBoolean(query, "compact");
            var dropZero = GetBoolean(query, "dropZero");
            var limit = int.TryParse(query?["maxResponseChars"].FirstOrDefault(), out var parsed)
                ? NormalizeLimit(parsed)
                : null;

            return new CompactionOptions(compact || dropZero || limit is not null, dropZero, limit);
        }

        private static bool GetBoolean(IQueryCollection? query, string name) =>
            bool.TryParse(query?[name].FirstOrDefault(), out var value) && value;
    }
}
