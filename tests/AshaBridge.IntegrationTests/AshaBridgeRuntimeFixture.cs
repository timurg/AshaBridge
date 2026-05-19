using AshaBridge.AspNetCore.Extensions;
using AshaBridge.Core.Runtime;
using AshaBridge.Sdk.Attributes;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AshaBridge.IntegrationTests;

public sealed class AshaBridgeRuntimeFixture : IDisposable
{
    private readonly ServiceProvider services;
    private readonly StreamingInvocationRuntime runtime;

    public AshaBridgeRuntimeFixture()
    {
        Config = IntegrationTestConfig.Load();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Config.AppSettingsPath, optional: false, reloadOnChange: false)
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddAshaBridge(configuration);

        services = serviceCollection.BuildServiceProvider(validateScopes: true);
        runtime = services.GetRequiredService<StreamingInvocationRuntime>();
    }

    public IntegrationTestConfig Config { get; }

    public Task<TResponse> InvokeAsync<TRequest, TResponse>(string methodName, TRequest request, CancellationToken ct = default)
        where TRequest : IMcpRequest<TResponse>
        => InvokeCoreAsync<TResponse>(methodName, request!, ct);

    public Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
        where TRequest : IMcpRequest<TResponse>
    {
        var methodName = typeof(TRequest).GetCustomAttributes(typeof(McpMethodAttribute), inherit: false)
            .OfType<McpMethodAttribute>()
            .SingleOrDefault()?.Name;

        Assert.False(string.IsNullOrWhiteSpace(methodName), $"{typeof(TRequest).Name} must declare {nameof(McpMethodAttribute)}.");
        return InvokeAsync<TRequest, TResponse>(methodName, request, ct);
    }

    public Task<TResponse> InvokeAsync<TResponse>(IMcpRequest<TResponse> request, CancellationToken ct = default)
    {
        var requestType = request.GetType();
        var methodName = requestType.GetCustomAttributes(typeof(McpMethodAttribute), inherit: false)
            .OfType<McpMethodAttribute>()
            .SingleOrDefault()?.Name;

        Assert.False(string.IsNullOrWhiteSpace(methodName), $"{requestType.Name} must declare {nameof(McpMethodAttribute)}.");
        return InvokeCoreAsync<TResponse>(methodName, request, ct);
    }

    private async Task<TResponse> InvokeCoreAsync<TResponse>(string methodName, object request, CancellationToken ct)
    {
        var execution = new AshaBridgeExecutionContext(
            correlationId: Guid.NewGuid().ToString("n"),
            userId: "integration-tests",
            organizationId: "default",
            tenantId: null,
            idempotencyKey: new IdempotencyKey(Guid.NewGuid().ToString("n")),
            permissions: Config.AllPermissions,
            services: services,
            requestAborted: ct);

        await foreach (var @event in runtime.InvokeAsync(methodName, request, execution, ct))
        {
            if (@event is MethodCompletedEvent<TResponse> completed)
            {
                return completed.Response;
            }

            if (@event is MethodFailedEvent failed)
            {
                Assert.Fail($"{failed.Error.Code}: {failed.Error.Message}");
            }
        }

        throw new InvalidOperationException($"AshaBridge method '{methodName}' completed without a response.");
    }

    public void Dispose() => services.Dispose();
}
