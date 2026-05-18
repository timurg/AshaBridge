using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AshaBridge.AspNetCore.Auth;

public sealed class AshaBridgeServiceTokenOptions : AuthenticationSchemeOptions
{
    public List<ServiceTokenDefinition> ServiceTokens { get; set; } = [];

    public SingleUserDefinition User { get; set; } = new();
}

public sealed class ServiceTokenDefinition
{
    public string Name { get; set; } = "";

    public string Token { get; set; } = "";

    public string? OrganizationId { get; set; }

    public string? TenantId { get; set; }

    public List<string> Permissions { get; set; } = [];
}

public sealed class SingleUserDefinition
{
    public string Username { get; set; } = "admin";

    public string Password { get; set; } = "";

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
        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateBearer(authorization));
        }

        if (authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateBasic(authorization));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }

    private AuthenticateResult AuthenticateBearer(string authorization)
    {
        var token = authorization["Bearer ".Length..].Trim();
        var serviceToken = Options.ServiceTokens.FirstOrDefault(t => FixedTimeEquals(t.Token, token));
        if (serviceToken is null)
        {
            return AuthenticateResult.Fail("Invalid service token.");
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

        return Succeed(serviceToken.Name, claims);
    }

    private AuthenticateResult AuthenticateBasic(string authorization)
    {
        var user = Options.User;
        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return AuthenticateResult.Fail("Single-user password authentication is not configured.");
        }

        string decoded;
        try
        {
            var credentials = Convert.FromBase64String(authorization["Basic ".Length..].Trim());
            decoded = Encoding.UTF8.GetString(credentials);
        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail("Invalid basic authentication header.");
        }

        var separator = decoded.IndexOf(':', StringComparison.Ordinal);
        if (separator < 0)
        {
            return AuthenticateResult.Fail("Invalid basic authentication credentials.");
        }

        var username = decoded[..separator];
        var password = decoded[(separator + 1)..];
        if (!FixedTimeEquals(user.Username, username) || !FixedTimeEquals(user.Password, password))
        {
            return AuthenticateResult.Fail("Invalid username or password.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Username),
            new(ClaimTypes.Name, user.Username),
            new("ashabridge.user", user.Username)
        };

        if (user.OrganizationId is not null)
        {
            claims.Add(new Claim("organization_id", user.OrganizationId));
        }

        if (user.TenantId is not null)
        {
            claims.Add(new Claim("tenant_id", user.TenantId));
        }

        claims.AddRange(user.Permissions.Select(p => new Claim("permission", p)));

        return Succeed(user.Username, claims);
    }

    private static AuthenticateResult Succeed(string name, List<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, SchemeName);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName));
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
