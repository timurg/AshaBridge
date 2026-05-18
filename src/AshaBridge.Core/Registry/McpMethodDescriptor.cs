using AshaBridge.Sdk.Attributes;

namespace AshaBridge.Core.Registry;

public sealed record McpMethodDescriptor(
    string Name,
    string ExtensionId,
    string ContractVersion,
    Type RequestType,
    Type ResponseType,
    Type HandlerType,
    bool IsStreaming,
    IReadOnlyCollection<string> Permissions,
    OperationRisk OperationRisk,
    CachePolicy? CachePolicy,
    bool RequiresIdempotency,
    string? Description,
    bool Enabled);

public sealed record CachePolicy(int TtlSeconds, CacheScope Scope);
