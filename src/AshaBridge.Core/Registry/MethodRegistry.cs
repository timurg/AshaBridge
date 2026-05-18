using System.Collections.Concurrent;

namespace AshaBridge.Core.Registry;

public sealed class MethodRegistry
{
    private readonly ConcurrentDictionary<string, McpMethodDescriptor> _methods = new(StringComparer.Ordinal);
    private volatile bool _frozen;

    public IReadOnlyCollection<McpMethodDescriptor> Methods => _methods.Values.ToArray();

    public bool IsFrozen => _frozen;

    public void Add(McpMethodDescriptor descriptor)
    {
        if (_frozen)
        {
            throw new InvalidOperationException("Method registry is immutable after startup.");
        }

        if (!_methods.TryAdd(descriptor.Name, descriptor))
        {
            throw new InvalidOperationException($"MCP method '{descriptor.Name}' is already registered.");
        }
    }

    public bool TryGet(string methodName, out McpMethodDescriptor descriptor) =>
        _methods.TryGetValue(methodName, out descriptor!);

    public void Freeze() => _frozen = true;
}
