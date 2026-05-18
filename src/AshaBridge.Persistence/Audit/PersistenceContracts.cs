using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace AshaBridge.Persistence.Audit;

public interface IMethodCallAuditStore
{
    Task AppendAsync(MethodCallAuditRecord record, CancellationToken ct);
}

public sealed record MethodCallAuditRecord(
    Guid Id,
    string CorrelationId,
    string? OrganizationId,
    string? UserId,
    string ExtensionId,
    string MethodName,
    string ContractVersion,
    string RequestHash,
    bool CacheHit,
    string Status,
    string? ErrorCode,
    string? ErrorMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt);

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> TryGetAsync(string key, CancellationToken ct);

    Task SaveAsync(IdempotencyRecord record, CancellationToken ct);
}

public sealed record IdempotencyRecord(
    Guid Id,
    string IdempotencyKey,
    string RequestHash,
    string ExtensionId,
    string MethodName,
    string Status,
    JsonNode? ResponseJson,
    string? ErrorCode,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed class InMemoryMethodCallAuditStore : IMethodCallAuditStore
{
    private readonly ConcurrentQueue<MethodCallAuditRecord> _records = new();

    public IReadOnlyCollection<MethodCallAuditRecord> Records => _records.ToArray();

    public Task AppendAsync(MethodCallAuditRecord record, CancellationToken ct)
    {
        _records.Enqueue(record);
        return Task.CompletedTask;
    }
}

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyRecord> _records = new(StringComparer.Ordinal);

    public Task<IdempotencyRecord?> TryGetAsync(string key, CancellationToken ct)
    {
        _records.TryGetValue(key, out var record);
        return Task.FromResult(record);
    }

    public Task SaveAsync(IdempotencyRecord record, CancellationToken ct)
    {
        _records[record.IdempotencyKey] = record;
        return Task.CompletedTask;
    }
}
