using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using AshaBridge.Core.Registry;
using AshaBridge.Core.Runtime;
using AshaBridge.Sdk.Attributes;
using AshaBridge.Sdk.Contracts;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;

namespace AshaBridge.AspNetCore.Extensions;

internal sealed class AshaBridgeMcpDispatcher(
    MethodRegistry methods,
    StreamingInvocationRuntime runtime,
    IHttpContextAccessor httpContextAccessor)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public ValueTask<ListToolsResult> ListToolsAsync(CancellationToken ct)
    {
        var locale = httpContextAccessor.HttpContext?.Request.Query["locale"].FirstOrDefault();
        var tools = methods.Methods
            .Where(method => method.Enabled && method.ExposeAsTool)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .Select(method => CreateTool(method, locale))
            .ToList();

        return ValueTask.FromResult(new ListToolsResult { Tools = tools });
    }

    public async ValueTask<CallToolResult> CallToolAsync(
        CallToolRequestParams? parameters,
        IServiceProvider requestServices,
        CancellationToken ct)
    {
        var methodName = parameters?.Name;
        if (methodName is null || !methods.TryGet(methodName, out var method) || !method.ExposeAsTool)
        {
            return Error($"MCP tool '{methodName}' was not found.");
        }

        object request;
        try
        {
            var arguments = parameters?.Arguments ?? new Dictionary<string, JsonElement>();
            request = JsonSerializer.Deserialize(JsonSerializer.Serialize(arguments, JsonOptions), method.RequestType, JsonOptions)
                ?? throw new JsonException("The tool request is empty.");
        }
        catch (JsonException ex)
        {
            return Error($"Invalid arguments for '{methodName}': {ex.Message}");
        }

        var http = httpContextAccessor.HttpContext;
        var execution = new AshaBridgeExecutionContext(
            correlationId: http?.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? http?.TraceIdentifier ?? Guid.NewGuid().ToString("n"),
            userId: http?.User.Identity?.Name,
            organizationId: http?.User.FindFirst("organization_id")?.Value,
            tenantId: http?.User.FindFirst("tenant_id")?.Value,
            idempotencyKey: GetIdempotencyKey(http, methodName, request),
            permissions: http?.User.FindAll("permission").Select(claim => claim.Value).ToArray() ?? [],
            services: requestServices,
            requestAborted: http?.RequestAborted ?? ct);

        await foreach (var @event in runtime.InvokeAsync(methodName, request, execution, ct).ConfigureAwait(false))
        {
            if (@event is MethodFailedEvent failed)
            {
                return Error($"{failed.Error.Code}: {failed.Error.Message}");
            }

            var eventType = @event.GetType();
            if (eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(MethodCompletedEvent<>))
            {
                return Success(eventType.GetProperty("Response")!.GetValue(@event));
            }
        }

        return Error($"MCP tool '{methodName}' completed without a response.");
    }

    private static Tool CreateTool(McpMethodDescriptor method, string? locale)
    {
        var description = method.Description;
        if (!string.IsNullOrWhiteSpace(locale))
        {
            description = method.RequestType
                .GetCustomAttributes<McpToolDescriptionAttribute>()
                .FirstOrDefault(candidate => LocaleMatches(candidate.Locale, locale))?
                .Description ?? description;
        }

        return new Tool
        {
            Name = method.Name,
            Description = description,
            InputSchema = BuildInputSchema(method.RequestType),
            Annotations = new ToolAnnotations
            {
                ReadOnlyHint = method.OperationRisk == OperationRisk.Read,
                DestructiveHint = method.OperationRisk == OperationRisk.WriteHigh,
                IdempotentHint = method.RequiresIdempotency,
                OpenWorldHint = true
            }
        };
    }

    private static JsonElement BuildInputSchema(Type requestType)
    {
        var schema = JsonSchemaExporter.GetJsonSchemaAsNode(JsonOptions, requestType, JsonSchemaExporterOptions.Default);
        schema["type"] = "object";
        schema.AsObject().Remove("$schema");
        if (schema["properties"] is JsonObject properties)
        {
            foreach (var property in requestType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var name = JsonOptions.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
                var description = property.GetCustomAttribute<McpParameterDescriptionAttribute>()?.Description;
                if (description is not null && properties[name] is JsonObject propertySchema)
                {
                    propertySchema["description"] = description;
                }
            }
        }

        return JsonSerializer.SerializeToElement(schema, JsonOptions);
    }

    private static CallToolResult Success(object? response)
    {
        var structured = JsonSerializer.SerializeToElement(response, response?.GetType() ?? typeof(object), JsonOptions);
        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = structured.GetRawText() }],
            StructuredContent = structured
        };
    }

    private static CallToolResult Error(string message) =>
        new() { IsError = true, Content = [new TextContentBlock { Text = message }] };

    private static IdempotencyKey GetIdempotencyKey(HttpContext? http, string methodName, object request)
    {
        var value = http?.Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(value))
        {
            return new IdempotencyKey(value);
        }

        var requestJson = JsonSerializer.Serialize(request, request.GetType(), JsonOptions);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{methodName}:{requestJson}")));
        return new IdempotencyKey($"mcp:{methodName}:{hash}");
    }

    private static bool LocaleMatches(string available, string requested) =>
        requested.Equals(available, StringComparison.OrdinalIgnoreCase)
        || requested.StartsWith($"{available}-", StringComparison.OrdinalIgnoreCase);
}
