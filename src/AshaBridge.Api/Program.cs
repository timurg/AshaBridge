using AshaBridge.AspNetCore.Extensions;
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

    builder.Services.AddAshaBridge(builder.Configuration);

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
