using Microsoft.AspNetCore.Builder;

namespace AshaBridge.AspNetCore.Extensions;

public static class AshaBridgeApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAshaBridge(this IApplicationBuilder app) => app;
}
