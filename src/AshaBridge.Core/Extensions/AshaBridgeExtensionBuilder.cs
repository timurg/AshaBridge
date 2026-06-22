using System.Reflection;
using AshaBridge.Core.Registry;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AshaBridge.Core.Extensions;

public sealed class AshaBridgeExtensionBuilder(
    IServiceCollection services,
    IConfiguration configuration,
    MethodRegistry methods,
    ContractRegistry contracts,
    string extensionId) : IAshaBridgeExtensionBuilder
{
    public IServiceCollection Services { get; } = services;

    public IConfiguration Configuration { get; } = configuration;

    public void AddMethod<TRequest, TResponse, THandler>()
        where TRequest : IMcpRequest<TResponse>
        where THandler : class, IMcpMethodHandler<TRequest, TResponse>
        => AddMethodCore<TRequest, TResponse, THandler>(exposeAsTool: false);

    public void AddToolMethod<TRequest, TResponse, THandler>()
        where TRequest : IMcpRequest<TResponse>
        where THandler : class, IMcpMethodHandler<TRequest, TResponse> =>
        AddMethodCore<TRequest, TResponse, THandler>(exposeAsTool: true);

    private void AddMethodCore<TRequest, TResponse, THandler>(bool exposeAsTool)
        where TRequest : IMcpRequest<TResponse>
        where THandler : class, IMcpMethodHandler<TRequest, TResponse>
    {
        var contract = contracts.Register(typeof(TRequest));
        ValidateResponse<TResponse>(contract);
        if (exposeAsTool)
        {
            ValidateToolMetadata(contract);
        }
        Services.AddTransient<THandler>();
        Services.AddTransient(typeof(IMcpMethodHandler<TRequest, TResponse>), typeof(THandler));
        methods.Add(ToMethodDescriptor<TRequest, TResponse, THandler>(contract, isStreaming: false, exposeAsTool));
    }

    public void AddStreamingMethod<TRequest, TResponse, THandler>()
        where TRequest : IMcpRequest<TResponse>
        where THandler : class, IStreamingMcpMethodHandler<TRequest, TResponse>
    {
        var contract = contracts.Register(typeof(TRequest));
        ValidateResponse<TResponse>(contract);
        Services.AddTransient<THandler>();
        Services.AddTransient(typeof(IStreamingMcpMethodHandler<TRequest, TResponse>), typeof(THandler));
        methods.Add(ToMethodDescriptor<TRequest, TResponse, THandler>(contract, isStreaming: true, exposeAsTool: false));
    }

    public void AddContractsFromAssembly(Assembly assembly) => contracts.AddContractsFromAssembly(assembly);

    private McpMethodDescriptor ToMethodDescriptor<TRequest, TResponse, THandler>(
        ContractDescriptor contract,
        bool isStreaming,
        bool exposeAsTool) =>
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
            exposeAsTool,
            Enabled: true);

    private static void ValidateResponse<TResponse>(ContractDescriptor contract)
    {
        if (contract.ResponseType != typeof(TResponse))
        {
            throw new InvalidOperationException($"{contract.RequestType.FullName} response type does not match handler response type.");
        }
    }

    private static void ValidateToolMetadata(ContractDescriptor contract)
    {
        if (string.IsNullOrWhiteSpace(contract.Description))
        {
            throw new InvalidOperationException($"AI tool '{contract.MethodName}' must have McpDescriptionAttribute.");
        }

        var undocumented = contract.Parameters
            .Where(parameter => string.IsNullOrWhiteSpace(parameter.Description))
            .Select(parameter => parameter.Name)
            .ToArray();
        if (undocumented.Length > 0)
        {
            throw new InvalidOperationException(
                $"AI tool '{contract.MethodName}' has parameters without descriptions: {string.Join(", ", undocumented)}");
        }
    }
}
