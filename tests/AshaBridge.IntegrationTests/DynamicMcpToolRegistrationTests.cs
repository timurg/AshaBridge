using AshaBridge.AspNetCore.Extensions;
using AshaBridge.Extensions.Bitrix24;
using AshaBridge.Extensions.Moodle;
using AshaBridge.PluginHost.Manifests;
using AshaBridge.Sdk.Attributes;
using AshaBridge.Sdk.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AshaBridge.IntegrationTests;

public sealed class DynamicMcpToolRegistrationTests
{
    [Fact]
    public async Task PluginFolderLoader_LoadsEnabledExtensionsFromManifests()
    {
        var extensionsPath = Path.Combine(AppContext.BaseDirectory, "extensions");
        var loader = new PluginFolderLoader(new ExtensionManifestReader());

        var plugins = await loader.LoadAsync(
            extensionsPath,
            ["ashabridge.extensions.bitrix24", "ashabridge.extensions.moodle"],
            CancellationToken.None);

        Assert.Equal(
            ["ashabridge.extensions.bitrix24", "ashabridge.extensions.moodle"],
            plugins.Select(plugin => plugin.Extension.Id).OrderBy(id => id, StringComparer.Ordinal));
    }

    [Fact]
    public async Task PluginTool_IsInvokedWithoutHostSpecificSurface()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddAshaBridge(configuration, [new EchoExtension()]);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext();
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<AshaBridgeMcpDispatcher>();

        var result = await dispatcher.CallToolAsync(
            new ModelContextProtocol.Protocol.CallToolRequestParams
            {
                Name = "test_echo",
                Arguments = new Dictionary<string, JsonElement>
                {
                    ["message"] = JsonSerializer.SerializeToElement("hello")
                }
            },
            scope.ServiceProvider,
            CancellationToken.None);

        Assert.NotEqual(true, result.IsError);
        Assert.Equal("hello", result.StructuredContent?.GetProperty("message").GetString());
    }

    [Fact]
    public async Task LoadedExtensions_DeterminePublishedToolsAndLocale()
    {
        var config = IntegrationTestConfig.Load();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(config.AppSettingsPath, optional: false, reloadOnChange: false)
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddAshaBridge(configuration, [new Bitrix24Extension(), new MoodleExtension()]);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var dispatcher = provider.GetRequiredService<AshaBridgeMcpDispatcher>();
        var http = provider.GetRequiredService<IHttpContextAccessor>();
        http.HttpContext = new DefaultHttpContext();

        var english = await dispatcher.ListToolsAsync(CancellationToken.None);
        var englishDescription = english.Tools.Single(tool => tool.Name == "moodle_user_find_by_email").Description;

        http.HttpContext.Request.QueryString = new QueryString("?locale=ru");
        var russian = await dispatcher.ListToolsAsync(CancellationToken.None);
        var russianDescription = russian.Tools.Single(tool => tool.Name == "moodle_user_find_by_email").Description;

        http.HttpContext.Request.QueryString = new QueryString("?locale=ru-RU");
        var russianRegional = await dispatcher.ListToolsAsync(CancellationToken.None);
        var russianRegionalDescription = russianRegional.Tools
            .Single(tool => tool.Name == "moodle_user_find_by_email")
            .Description;

        http.HttpContext.Request.QueryString = new QueryString("?locale=unknown");
        var fallback = await dispatcher.ListToolsAsync(CancellationToken.None);
        var fallbackDescription = fallback.Tools.Single(tool => tool.Name == "moodle_user_find_by_email").Description;

        Assert.Equal(32, english.Tools.Count);
        Assert.Contains(english.Tools, tool => tool.Name == "moodle_user_create");
        Assert.StartsWith("Find one Moodle user", englishDescription, StringComparison.Ordinal);
        Assert.StartsWith("Найти пользователя Moodle", russianDescription, StringComparison.Ordinal);
        Assert.Equal(russianDescription, russianRegionalDescription);
        Assert.Equal(englishDescription, fallbackDescription);
        Assert.All(english.Tools, AssertToolMetadata);
        Assert.DoesNotContain(english.Tools, tool => tool.Name == "bitrix_crm_item_update");
    }

    private static void AssertToolMetadata(ModelContextProtocol.Protocol.Tool tool)
    {
        Assert.False(string.IsNullOrWhiteSpace(tool.Description), $"Tool '{tool.Name}' has no description.");
        if (!tool.InputSchema.TryGetProperty("properties", out var properties))
        {
            return;
        }

        foreach (var property in properties.EnumerateObject())
        {
            Assert.True(
                property.Value.TryGetProperty("description", out var description)
                    && !string.IsNullOrWhiteSpace(description.GetString()),
                $"Tool '{tool.Name}' parameter '{property.Name}' has no description.");
        }
    }

    private sealed class EchoExtension : IAshaBridgeExtension
    {
        public string Id => "test.echo";

        public string Version => "1.0.0";

        public void Configure(IAshaBridgeExtensionBuilder builder)
        {
            builder.Services.AddScoped<EchoDependency>();
            builder.AddToolMethod<EchoRequest, EchoResponse, EchoHandler>();
        }
    }

    [McpMethod("test_echo")]
    [McpDescription("Echo a message.")]
    private sealed record EchoRequest(
        [property: McpParameterDescription("Message to echo.")]
        string Message) : IMcpRequest<EchoResponse>;

    private sealed record EchoResponse(string Message);

    private sealed class EchoDependency;

    private sealed class EchoHandler(EchoDependency dependency) : IMcpMethodHandler<EchoRequest, EchoResponse>
    {
        public Task<EchoResponse> HandleAsync(EchoRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
            Task.FromResult(new EchoResponse(dependency is not null ? request.Message : string.Empty));
    }
}
