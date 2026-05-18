namespace AshaBridge.Core.Extensions;

public sealed class ExtensionRegistry
{
    private readonly List<ExtensionDescriptor> _extensions = [];
    private bool _frozen;

    public IReadOnlyCollection<ExtensionDescriptor> Extensions => _extensions.ToArray();

    public bool IsFrozen => _frozen;

    public void Add(ExtensionDescriptor descriptor)
    {
        if (_frozen)
        {
            throw new InvalidOperationException("Extension registry is immutable after startup.");
        }

        if (_extensions.Any(e => e.Id == descriptor.Id))
        {
            throw new InvalidOperationException($"Extension '{descriptor.Id}' is already registered.");
        }

        _extensions.Add(descriptor);
    }

    public void Freeze() => _frozen = true;
}

public sealed record ExtensionDescriptor(string Id, string Version, bool Enabled, string LoadMode);
