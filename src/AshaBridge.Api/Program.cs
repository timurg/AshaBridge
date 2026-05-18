using AshaBridge.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAshaBridge(builder.Configuration);

var app = builder.Build();

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
