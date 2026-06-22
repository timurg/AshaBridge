using AshaBridge.AspNetCore.Extensions;
using AshaBridge.PluginHost.Manifests;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("appsettings.Logging.json", optional: false, reloadOnChange: true);
    builder.Configuration.AddEnvironmentVariables();
    var logFilePath = builder.Configuration["AshaBridgeLogging:FilePath"] ?? "logs/ashabridge-.log";
    var fileLoggingEnabled = TryPrepareLogDirectory(logFilePath);
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();

        if (fileLoggingEnabled)
        {
            configuration.WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                fileSizeLimitBytes: 104857600,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1));
        }
    });

    var enabledExtensionIds = builder.Configuration
        .GetSection("ashabridge:extensions:enabled")
        .Get<string[]>() ?? [];
    var configuredExtensionsPath = builder.Configuration["ashabridge:extensions:path"] ?? "./extensions";
    var extensionsPath = Path.GetFullPath(configuredExtensionsPath, builder.Environment.ContentRootPath);
    if (!Directory.Exists(extensionsPath))
    {
        extensionsPath = Path.GetFullPath(configuredExtensionsPath, AppContext.BaseDirectory);
    }
    var pluginLoader = new PluginFolderLoader(new ExtensionManifestReader());
    var plugins = await pluginLoader.LoadAsync(extensionsPath, enabledExtensionIds, CancellationToken.None);
    var missingExtensionIds = enabledExtensionIds
        .Except(plugins.Select(plugin => plugin.Extension.Id), StringComparer.Ordinal)
        .ToArray();
    if (missingExtensionIds.Length > 0)
    {
        throw new InvalidOperationException(
            $"Enabled extensions were not loaded from '{extensionsPath}': {string.Join(", ", missingExtensionIds)}");
    }

    builder.Services.AddAshaBridge(builder.Configuration, plugins.Select(plugin => plugin.Extension));

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAshaBridge();

    app.MapGet("/", () => Results.Ok(new
    {
        name = "AshaBridge",
        status = "running"
    }));

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/ready");
    app.MapAshaBridgeMcp("/mcp");
    app.MapAshaBridgeDiagnostics("/internal/ashabridge");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AshaBridge terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static bool TryPrepareLogDirectory(string logFilePath)
{
    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(logFilePath))!);
        return true;
    }
    catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
    {
        Log.Warning(ex, "File logging is disabled because the log directory is not writable. FilePath={FilePath}", logFilePath);
        return false;
    }
}
