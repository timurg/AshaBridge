using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AshaBridge.Sdk.Contracts;

public interface IMcpRequest<TResponse>
{
}

public interface IMcpMethodHandler<in TRequest, TResponse>
    where TRequest : IMcpRequest<TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        IAshaBridgeExecutionContext execution,
        CancellationToken ct);
}

public interface IStreamingMcpMethodHandler<in TRequest, TResponse>
    where TRequest : IMcpRequest<TResponse>
{
    IAsyncEnumerable<AshaBridgeInvocationEvent> HandleStreamAsync(
        TRequest request,
        IAshaBridgeExecutionContext execution,
        CancellationToken ct);
}

public interface IAshaBridgeExtension
{
    string Id { get; }

    string Version { get; }

    void Configure(IAshaBridgeExtensionBuilder builder);
}

public interface IAshaBridgeExtensionBuilder
{
    IServiceCollection Services { get; }

    void AddMethod<TRequest, TResponse, THandler>()
        where TRequest : IMcpRequest<TResponse>
        where THandler : class, IMcpMethodHandler<TRequest, TResponse>;

    void AddStreamingMethod<TRequest, TResponse, THandler>()
        where TRequest : IMcpRequest<TResponse>
        where THandler : class, IStreamingMcpMethodHandler<TRequest, TResponse>;

    void AddContractsFromAssembly(Assembly assembly);
}

public interface IAshaBridgeExecutionContext
{
    string CorrelationId { get; }

    string? UserId { get; }

    string? OrganizationId { get; }

    string? TenantId { get; }

    IdempotencyKey? IdempotencyKey { get; }

    IReadOnlyCollection<string> Permissions { get; }

    IServiceProvider Services { get; }

    CancellationToken RequestAborted { get; }
}

public sealed record IdempotencyKey(string Value);
