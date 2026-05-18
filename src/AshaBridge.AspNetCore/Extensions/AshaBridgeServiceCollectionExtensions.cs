using AshaBridge.AspNetCore.Auth;
using AshaBridge.Caching;
using AshaBridge.Core.Extensions;
using AshaBridge.Core.Options;
using AshaBridge.Core.Registry;
using AshaBridge.Core.Runtime;
using AshaBridge.Extensions.Bitrix24;
using AshaBridge.Extensions.Moodle;
using AshaBridge.Persistence.Audit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            .WithTools<AshaBridgeMcpToolSurface>();

        RegisterBuiltInExtensions(services, configuration, methods, contracts, extensions);
        services.AddSingleton<IAshaBridgeStartupMarker, AshaBridgeStartupMarker>();

        return services;
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
