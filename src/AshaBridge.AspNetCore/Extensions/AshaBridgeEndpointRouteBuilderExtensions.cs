using AshaBridge.Core.Extensions;
using AshaBridge.Core.Registry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AshaBridge.AspNetCore.Extensions;

public static class AshaBridgeEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAshaBridgeMcp(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/mcp")
    {
        global::Microsoft.AspNetCore.Builder.McpEndpointRouteBuilderExtensions.MapMcp(endpoints, pattern)
            .RequireAuthorization();
        return endpoints;
    }

    public static IEndpointRouteBuilder MapAshaBridgeDiagnostics(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/internal/ashabridge")
    {
        var group = endpoints.MapGroup(pattern).RequireAuthorization();

        group.MapGet("/methods", (MethodRegistry registry) => Results.Ok(registry.Methods.Select(m => new
        {
            m.Name,
            m.ExtensionId,
            m.ContractVersion,
            RequestType = m.RequestType.FullName,
            ResponseType = m.ResponseType.FullName,
            m.IsStreaming,
            m.Permissions,
            OperationRisk = m.OperationRisk.ToString(),
            m.RequiresIdempotency,
            m.Description,
            m.Enabled
        })));

        group.MapGet("/extensions", (ExtensionRegistry registry) => Results.Ok(registry.Extensions));
        group.MapGet("/contracts", (ContractRegistry registry) => Results.Ok(registry.Contracts.Select(c => new
        {
            c.MethodName,
            c.Version,
            RequestType = c.RequestType.FullName,
            ResponseType = c.ResponseType.FullName,
            c.Permissions,
            OperationRisk = c.OperationRisk.ToString(),
            c.RequiresIdempotency,
            c.Description,
            Parameters = c.Parameters.Select(p => new
            {
                p.Name,
                Type = p.Type.FullName,
                p.Description,
                p.IsCacheKey
            })
        })));

        return endpoints;
    }
}
