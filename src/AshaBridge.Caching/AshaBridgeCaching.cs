using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AshaBridge.Core.Registry;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace AshaBridge.Caching;

public interface IAshaBridgeCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken ct);
}

public sealed class MemoryAshaBridgeCache(IMemoryCache cache) : IAshaBridgeCache
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken ct)
    {
        if (cache.TryGetValue<T>(key, out var cached))
        {
            return cached!;
        }

        var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            if (cache.TryGetValue<T>(key, out cached))
            {
                return cached!;
            }

            var value = await factory(ct).ConfigureAwait(false);
            cache.Set(key, value, ttl);
            return value;
        }
        finally
        {
            gate.Release();
            _locks.TryRemove(key, out _);
        }
    }
}

public sealed class CacheKeyBuilder
{
    public string Build(
        McpMethodDescriptor method,
        object request,
        IAshaBridgeExecutionContext execution)
    {
        var parts = new List<string>
        {
            $"method={method.Name}",
            $"version={method.ContractVersion}",
            $"extension={method.ExtensionId}",
            $"organization={execution.OrganizationId ?? "none"}",
            $"tenant={execution.TenantId ?? "none"}"
        };

        foreach (var property in request.GetType().GetProperties())
        {
            var value = property.GetValue(request);
            parts.Add($"{property.Name}={JsonSerializer.Serialize(value)}");
        }

        var canonical = string.Join("|", parts.Order(StringComparer.Ordinal));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
