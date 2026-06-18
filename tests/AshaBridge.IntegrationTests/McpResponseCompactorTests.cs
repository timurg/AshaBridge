using System.Text.Json;
using System.Text.Json.Nodes;
using AshaBridge.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;

namespace AshaBridge.IntegrationTests;

public sealed class McpResponseCompactorTests
{
    [Fact]
    public void Compact_UsesEndpointQueryForTextAndStructuredContent()
    {
        const string input = "{\"deal\":{\"ID\":\"46\",\"EMPTY\":\"\",\"ZERO\":0}}";
        var result = new CallToolResult
        {
            Content = [new TextContentBlock { Text = input }],
            StructuredContent = JsonSerializer.Deserialize<JsonElement>(input)
        };
        var http = new DefaultHttpContext();
        http.Request.QueryString = new QueryString("?compact=true&dropZero=true&maxResponseChars=1000");

        McpResponseCompactor.Compact(result, http, "test_tool", NullLoggerFactory.Instance);

        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        Assert.Equal("{\"deal\":{\"ID\":\"46\"}}", text);
        Assert.Equal(text, result.StructuredContent?.GetRawText());
    }

    [Fact]
    public void CompactText_RemovesEmptyValuesAndOptionalZeros()
    {
        const string input = """
            {
              "nullValue": null,
              "emptyString": "",
              "whitespace": "  ",
              "zero": 0,
              "falseValue": false,
              "nested": {
                "emptyArray": [],
                "name": "kept"
              }
            }
            """;

        var compacted = McpResponseCompactor.CompactText(input, dropZero: true, maxResponseChars: null);
        var result = JsonNode.Parse(compacted)!.AsObject();

        Assert.False(result.ContainsKey("nullValue"));
        Assert.False(result.ContainsKey("emptyString"));
        Assert.False(result.ContainsKey("whitespace"));
        Assert.False(result.ContainsKey("zero"));
        Assert.False(result["falseValue"]!.GetValue<bool>());
        Assert.Equal("kept", result["nested"]?["name"]?.GetValue<string>());
        Assert.Null(result["nested"]?["emptyArray"]);
    }

    [Fact]
    public void CompactText_EnforcesLimitAndKeepsValidJson()
    {
        var input = new JsonObject
        {
            ["deal"] = new JsonObject(
                Enumerable.Range(1, 30)
                    .Select(index => KeyValuePair.Create<string, JsonNode?>(
                        $"UF_FIELD_{index}",
                        JsonValue.Create(new string((char)('a' + index % 20), 300)))))
        }.ToJsonString();

        var compacted = McpResponseCompactor.CompactText(input, dropZero: false, maxResponseChars: 600);
        var result = JsonNode.Parse(compacted)!.AsObject();

        Assert.True(compacted.Length <= 600);
        Assert.True(result["_ashabridge"]?["truncated"]?.GetValue<bool>());
        Assert.Equal(600, result["_ashabridge"]?["maxChars"]?.GetValue<int>());
    }
}
