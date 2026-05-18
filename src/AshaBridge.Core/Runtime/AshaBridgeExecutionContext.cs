using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Core.Runtime;

public sealed class AshaBridgeExecutionContext(
    string correlationId,
    string? userId,
    string? organizationId,
    string? tenantId,
    IdempotencyKey? idempotencyKey,
    IReadOnlyCollection<string> permissions,
    IServiceProvider services,
    CancellationToken requestAborted) : IAshaBridgeExecutionContext
{
    public string CorrelationId { get; } = correlationId;

    public string? UserId { get; } = userId;

    public string? OrganizationId { get; } = organizationId;

    public string? TenantId { get; } = tenantId;

    public IdempotencyKey? IdempotencyKey { get; } = idempotencyKey;

    public IReadOnlyCollection<string> Permissions { get; } = permissions;

    public IServiceProvider Services { get; } = services;

    public CancellationToken RequestAborted { get; } = requestAborted;
}
