using System.Text.Json;
using System.Text.Json.Nodes;
using AshaBridge.AspNetCore.Auth;
using AshaBridge.Caching;
using AshaBridge.Core.Extensions;
using AshaBridge.Core.Options;
using AshaBridge.Core.Registry;
using AshaBridge.Core.Runtime;
using AshaBridge.Extensions.Bitrix24;
using AshaBridge.Extensions.Moodle;
using AshaBridge.Persistence.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace AshaBridge.AspNetCore.Extensions;

public static class AshaBridgeServiceCollectionExtensions
{
    public static IServiceCollection AddAshaBridge(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AshaBridgeOptions>()
            .Bind(configuration.GetSection("ashabridge"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<BitrixExtensionOptions>()
            .Bind(configuration.GetSection("bitrix"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<MoodleExtensionOptions>()
            .Bind(configuration.GetSection("moodle"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var methods = new MethodRegistry();
        var contracts = new ContractRegistry();
        var extensions = new ExtensionRegistry();
        services.AddSingleton(methods);
        services.AddSingleton(contracts);
        services.AddSingleton(extensions);
        services.AddSingleton<StreamingInvocationRuntime>();
        services.AddSingleton<CacheKeyBuilder>();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddSingleton<IAshaBridgeCache, MemoryAshaBridgeCache>();
        services.AddSingleton<IMethodCallAuditStore, InMemoryMethodCallAuditStore>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        services
            .AddAuthentication(AshaBridgeServiceTokenHandler.SchemeName)
            .AddScheme<AshaBridgeServiceTokenOptions, AshaBridgeServiceTokenHandler>(
                AshaBridgeServiceTokenHandler.SchemeName,
                options => configuration.GetSection("security").Bind(options));

        services.AddAuthorization();
        services.AddHealthChecks()
            .AddCheck<AshaBridgeRegistryHealthCheck>("ashabridge_registries")
            .AddCheck<AshaBridgeExtensionsHealthCheck>("ashabridge_extensions");

        services.AddMcpServer()
            .WithHttpTransport()
            .WithRequestFilters(filters =>
            {
                filters.AddListToolsFilter(next => async (request, ct) =>
                {
                    var result = await next(request, ct).ConfigureAwait(false);
                    AddN8nValueCompatibilityToToolSchemas(result.Tools);
                    return result;
                });
                filters.AddCallToolFilter(next => async (request, ct) =>
                {
                    UnwrapSingleValueObjectArgument(request.Params);
                    var result = await next(request, ct).ConfigureAwait(false);
                    var requestServices = request.Services;
                    McpResponseCompactor.Compact(
                        result,
                        requestServices?.GetService<IHttpContextAccessor>()?.HttpContext,
                        request.Params?.Name,
                        requestServices?.GetService<ILoggerFactory>());
                    return result;
                });
            })
            .WithTools<AshaBridgeMcpToolSurface>();

        RegisterBuiltInExtensions(services, configuration, methods, contracts, extensions);
        services.AddSingleton<IAshaBridgeStartupMarker, AshaBridgeStartupMarker>();

        return services;
    }

    private static void AddN8nValueCompatibilityToToolSchemas(IEnumerable<Tool> tools)
    {
        foreach (var tool in tools)
        {
            if (!IsN8nValueCompatibleLookupTool(tool.Name))
            {
                continue;
            }

            var schema = JsonNode.Parse(tool.InputSchema.GetRawText()) as JsonObject;
            var properties = schema?["properties"] as JsonObject;
            if (schema is null || properties is null || properties.ContainsKey("value"))
            {
                continue;
            }

            properties["value"] = BuildN8nValueSchema(tool.Name);
            tool.InputSchema = JsonSerializer.SerializeToElement(schema);
        }
    }

    private static bool IsN8nValueCompatibleLookupTool(string? toolName) =>
        toolName is "moodle_user_find_by_email"
            or "moodle_user_find_by_id"
            or "moodle_user_find_by_username";

    private static JsonObject BuildN8nValueSchema(string? toolName)
    {
        var properties = new JsonObject
        {
            ["value"] = new JsonObject { ["type"] = "string" },
            ["query"] = new JsonObject { ["type"] = "string" }
        };

        properties[toolName switch
        {
            "moodle_user_find_by_id" => "id",
            "moodle_user_find_by_username" => "username",
            _ => "email"
        }] = new JsonObject { ["type"] = "string" };

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["additionalProperties"] = true
        };
    }

    private static void UnwrapSingleValueObjectArgument(CallToolRequestParams? parameters)
    {
        var arguments = parameters?.Arguments;
        if (arguments is not { Count: 1 }
            || !arguments.TryGetValue("value", out var value)
            || value.ValueKind is not JsonValueKind.Object)
        {
            return;
        }

        parameters!.Arguments = value.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal);
    }

    private static void RegisterBuiltInExtensions(
        IServiceCollection services,
        IConfiguration configuration,
        MethodRegistry methods,
        ContractRegistry contracts,
        ExtensionRegistry extensions)
    {
        var enabled = configuration.GetSection("ashabridge:extensions:enabled").Get<string[]>()
            ?? ["ashabridge.extensions.bitrix24", "ashabridge.extensions.moodle"];

        RegisterIfEnabled(services, enabled, methods, contracts, extensions, new Bitrix24Extension());
        RegisterIfEnabled(services, enabled, methods, contracts, extensions, new MoodleExtension());

        methods.Freeze();
        contracts.Freeze();
        extensions.Freeze();
    }

    private static void RegisterIfEnabled(
        IServiceCollection services,
        IReadOnlyCollection<string> enabled,
        MethodRegistry methods,
        ContractRegistry contracts,
        ExtensionRegistry extensions,
        Sdk.Contracts.IAshaBridgeExtension extension)
    {
        var isEnabled = enabled.Contains(extension.Id, StringComparer.Ordinal);
        extensions.Add(new ExtensionDescriptor(extension.Id, extension.Version, isEnabled, "built-in"));
        if (!isEnabled)
        {
            return;
        }

        extension.Configure(new AshaBridgeExtensionBuilder(services, methods, contracts, extension.Id));
    }
}

public interface IAshaBridgeStartupMarker;

public sealed class AshaBridgeStartupMarker : IAshaBridgeStartupMarker;
