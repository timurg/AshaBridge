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
            var assemblyPath = Directory.EnumerateFiles(directory, "*.dll").FirstOrDefault();
            if (assemblyPath is null)
            {
                continue;
            }

            var context = new AssemblyLoadContext($"AshaBridge:{manifest.Id}", isCollectible: false);
            var assembly = context.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
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
}

public sealed record LoadedPluginExtension(
    ExtensionManifest Manifest,
    IAshaBridgeExtension Extension,
    Assembly Assembly);
