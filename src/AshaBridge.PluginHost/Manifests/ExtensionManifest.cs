using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.PluginHost.Manifests;

public sealed record ExtensionManifest(
    string Id,
    string Name,
    string Version,
    string SdkVersion,
    string Assembly,
    IReadOnlyList<string> ProvidesMethods,
    IReadOnlyList<string> RequiresConfiguration);

public sealed class ExtensionManifestReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ExtensionManifest> ReadAsync(string manifestPath, CancellationToken ct)
    {
        await using var stream = File.OpenRead(manifestPath);
        return await JsonSerializer.DeserializeAsync<ExtensionManifest>(stream, JsonOptions, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Extension manifest '{manifestPath}' is empty.");
    }
}

public sealed class PluginFolderLoader(ExtensionManifestReader manifestReader)
{
    public async Task<IReadOnlyList<LoadedPluginExtension>> LoadAsync(
        string extensionsPath,
        IReadOnlyCollection<string> enabledExtensionIds,
        CancellationToken ct)
    {
        if (!Directory.Exists(extensionsPath))
        {
            return [];
        }

        var loaded = new List<LoadedPluginExtension>();
        foreach (var manifestPath in Directory.EnumerateFiles(extensionsPath, "extension.manifest.json", SearchOption.AllDirectories))
        {
            var manifest = await manifestReader.ReadAsync(manifestPath, ct).ConfigureAwait(false);
            if (!enabledExtensionIds.Contains(manifest.Id, StringComparer.Ordinal))
            {
                continue;
            }

            if (!IsSdkCompatible(manifest.SdkVersion))
            {
                continue;
            }

            var directory = Path.GetDirectoryName(manifestPath)!;
            var assemblyPath = ResolveAssemblyPath(directory, manifest.Assembly);
            if (assemblyPath is null)
            {
                continue;
            }

            var context = new PluginLoadContext(manifest.Id, assemblyPath);
            var assembly = context.LoadFromAssemblyPath(assemblyPath);
            var extension = assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(IAshaBridgeExtension).IsAssignableFrom(t))
                .Select(t => (IAshaBridgeExtension?)Activator.CreateInstance(t))
                .FirstOrDefault(e => e is not null);

            if (extension is not null)
            {
                loaded.Add(new LoadedPluginExtension(manifest, extension, assembly));
            }
        }

        return loaded;
    }

    private static bool IsSdkCompatible(string sdkVersion) =>
        sdkVersion.Contains("1.0.0", StringComparison.Ordinal) ||
        sdkVersion.StartsWith(">=", StringComparison.Ordinal);

    private static string? ResolveAssemblyPath(string pluginDirectory, string assemblyName)
    {
        var pluginPath = Path.GetFullPath(Path.Combine(pluginDirectory, assemblyName));
        if (File.Exists(pluginPath))
        {
            return pluginPath;
        }

        var applicationPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, assemblyName));
        return File.Exists(applicationPath) ? applicationPath : null;
    }
}

public sealed record LoadedPluginExtension(
    ExtensionManifest Manifest,
    IAshaBridgeExtension Extension,
    Assembly Assembly);

internal sealed class PluginLoadContext(string pluginId, string mainAssemblyPath)
    : AssemblyLoadContext($"AshaBridge:{pluginId}", isCollectible: false)
{
    private readonly AssemblyDependencyResolver resolver = new(mainAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var shared = Default.Assemblies.FirstOrDefault(
            assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), assemblyName));
        if (shared is not null)
        {
            return shared;
        }

        var path = resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
    }
}
