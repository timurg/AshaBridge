using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AshaBridge.AspNetCore.Auth;

public sealed class AshaBridgeServiceTokenOptions : AuthenticationSchemeOptions
{
    public List<ServiceTokenDefinition> ServiceTokens { get; set; } = [];
}

public sealed class ServiceTokenDefinition
{
    public string Name { get; set; } = "";

    public string Token { get; set; } = "";

    public string? OrganizationId { get; set; }

    public string? TenantId { get; set; }

    public List<string> Permissions { get; set; } = [];
}

public sealed class AshaBridgeServiceTokenHandler(
    IOptionsMonitor<AshaBridgeServiceTokenOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AshaBridgeServiceTokenOptions>(options, logger, encoder)
{
    public const string SchemeName = "AshaBridgeServiceToken";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = authorization["Bearer ".Length..].Trim();
        var serviceToken = Options.ServiceTokens.FirstOrDefault(t => FixedTimeEquals(t.Token, token));
        if (serviceToken is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid service token."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, serviceToken.Name),
            new("ashabridge.service_token", serviceToken.Name)
        };

        if (serviceToken.OrganizationId is not null)
        {
            claims.Add(new Claim("organization_id", serviceToken.OrganizationId));
        }

        if (serviceToken.TenantId is not null)
        {
            claims.Add(new Claim("tenant_id", serviceToken.TenantId));
        }

        claims.AddRange(serviceToken.Permissions.Select(p => new Claim("permission", p)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        if (expected.Length != actual.Length)
        {
            return false;
        }

        var diff = 0;
        for (var i = 0; i < expected.Length; i++)
        {
            diff |= expected[i] ^ actual[i];
        }

        return diff == 0;
    }
}
