using System.Reflection;
using AshaBridge.Core.Registry;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AshaBridge.Core.Extensions;

public sealed class AshaBridgeExtensionBuilder(
    IServiceCollection services,
    MethodRegistry methods,
    ContractRegistry contracts,
    string extensionId) : IAshaBridgeExtensionBuilder
{
    public IServiceCollection Services { get; } = services;

    public void AddMethod<TRequest, TResponse, THandler>()
        where TRequest : IMcpRequest<TResponse>
        where THandler : class, IMcpMethodHandler<TRequest, TResponse>
    {
        var contract = contracts.Register(typeof(TRequest));
        ValidateResponse<TResponse>(contract);
        Services.AddTransient<THandler>();
        Services.AddTransient(typeof(IMcpMethodHandler<TRequest, TResponse>), typeof(THandler));
        methods.Add(ToMethodDescriptor<TRequest, TResponse, THandler>(contract, isStreaming: false));
    }

    public void AddStreamingMethod<TRequest, TResponse, THandler>()
        where TRequest : IMcpRequest<TResponse>
        where THandler : class, IStreamingMcpMethodHandler<TRequest, TResponse>
    {
        var contract = contracts.Register(typeof(TRequest));
        ValidateResponse<TResponse>(contract);
        Services.AddTransient<THandler>();
        Services.AddTransient(typeof(IStreamingMcpMethodHandler<TRequest, TResponse>), typeof(THandler));
        methods.Add(ToMethodDescriptor<TRequest, TResponse, THandler>(contract, isStreaming: true));
    }

    public void AddContractsFromAssembly(Assembly assembly) => contracts.AddContractsFromAssembly(assembly);

    private McpMethodDescriptor ToMethodDescriptor<TRequest, TResponse, THandler>(
        ContractDescriptor contract,
        bool isStreaming) =>
        new(
            contract.MethodName,
            extensionId,
            contract.Version,
            typeof(TRequest),
            typeof(TResponse),
            typeof(THandler),
            isStreaming,
            contract.Permissions,
            contract.OperationRisk,
            contract.CachePolicy,
            contract.RequiresIdempotency,
            contract.Description,
            Enabled: true);

    private static void ValidateResponse<TResponse>(ContractDescriptor contract)
    {
        if (contract.ResponseType != typeof(TResponse))
        {
            throw new InvalidOperationException($"{contract.RequestType.FullName} response type does not match handler response type.");
        }
    }
}
