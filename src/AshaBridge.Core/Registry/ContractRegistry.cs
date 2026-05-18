using System.Reflection;
using System.ComponentModel;
using AshaBridge.Sdk.Attributes;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Core.Registry;

public sealed class ContractRegistry
{
    private readonly Dictionary<Type, ContractDescriptor> _contracts = [];
    private bool _frozen;

    public IReadOnlyCollection<ContractDescriptor> Contracts => _contracts.Values.ToArray();

    public bool IsFrozen => _frozen;

    public ContractDescriptor Register(Type requestType)
    {
        if (_frozen)
        {
            throw new InvalidOperationException("Contract registry is immutable after startup.");
        }

        if (_contracts.TryGetValue(requestType, out var existing))
        {
            return existing;
        }

        var requestInterface = requestType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMcpRequest<>))
            ?? throw new InvalidOperationException($"{requestType.FullName} must implement IMcpRequest<TResponse>.");

        var method = requestType.GetCustomAttribute<McpMethodAttribute>()
            ?? throw new InvalidOperationException($"{requestType.FullName} must have McpMethodAttribute.");
        var version = requestType.GetCustomAttribute<ContractVersionAttribute>()?.Version ?? "1.0.0";
        var risk = requestType.GetCustomAttribute<OperationRiskAttribute>()?.Risk ?? OperationRisk.Read;
        var requiresIdempotency = requestType.GetCustomAttribute<RequiresIdempotencyAttribute>() is not null;

        if ((risk is OperationRisk.WriteMedium or OperationRisk.WriteHigh) && !requiresIdempotency)
        {
            throw new InvalidOperationException($"{method.Name} is {risk} and must require idempotency.");
        }

        var cacheable = requestType.GetCustomAttribute<CacheableAttribute>();
        var doNotCache = requestType.GetCustomAttribute<DoNotCacheAttribute>() is not null;
        var descriptor = new ContractDescriptor(
            MethodName: method.Name,
            Version: version,
            RequestType: requestType,
            ResponseType: requestInterface.GetGenericArguments()[0],
            Permissions: requestType.GetCustomAttributes<RequiresPermissionAttribute>().Select(p => p.Permission).ToArray(),
            OperationRisk: risk,
            CachePolicy: cacheable is null || doNotCache ? null : new CachePolicy(cacheable.TtlSeconds, cacheable.Scope),
            RequiresIdempotency: requiresIdempotency,
            Description: requestType.GetCustomAttribute<McpDescriptionAttribute>()?.Description,
            Parameters: requestType.GetProperties()
                .Select(p => new ContractParameterDescriptor(
                    p.Name,
                    p.PropertyType,
                    p.GetCustomAttribute<McpParameterDescriptionAttribute>()?.Description
                        ?? p.GetCustomAttribute<DescriptionAttribute>()?.Description,
                    p.GetCustomAttribute<CacheKeyAttribute>() is not null))
                .ToArray(),
            CacheKeyProperties: requestType.GetProperties().Where(p => p.GetCustomAttribute<CacheKeyAttribute>() is not null).ToArray());

        _contracts.Add(requestType, descriptor);
        return descriptor;
    }

    public void AddContractsFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && t.GetCustomAttribute<McpMethodAttribute>() is not null))
        {
            Register(type);
        }
    }

    public ContractDescriptor Get(Type requestType) => _contracts[requestType];

    public void Freeze() => _frozen = true;
}

public sealed record ContractDescriptor(
    string MethodName,
    string Version,
    Type RequestType,
    Type ResponseType,
    IReadOnlyCollection<string> Permissions,
    OperationRisk OperationRisk,
    CachePolicy? CachePolicy,
    bool RequiresIdempotency,
    string? Description,
    IReadOnlyCollection<ContractParameterDescriptor> Parameters,
    IReadOnlyCollection<PropertyInfo> CacheKeyProperties);

public sealed record ContractParameterDescriptor(
    string Name,
    Type Type,
    string? Description,
    bool IsCacheKey);
