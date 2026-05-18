using AshaBridge.Core.Extensions;
using AshaBridge.Core.Registry;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AshaBridge.AspNetCore;

public sealed class AshaBridgeRegistryHealthCheck(
    MethodRegistry methods,
    ContractRegistry contracts) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var healthy = methods.IsFrozen && contracts.IsFrozen && methods.Methods.All(m => m.Enabled);
        return Task.FromResult(healthy
            ? HealthCheckResult.Healthy("AshaBridge registries are frozen and ready.")
            : HealthCheckResult.Unhealthy("AshaBridge registries are not ready."));
    }
}

public sealed class AshaBridgeExtensionsHealthCheck(ExtensionRegistry extensions) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(extensions.IsFrozen
            ? HealthCheckResult.Healthy("AshaBridge extensions are loaded.")
            : HealthCheckResult.Unhealthy("AshaBridge extensions are not ready."));
    }
}
