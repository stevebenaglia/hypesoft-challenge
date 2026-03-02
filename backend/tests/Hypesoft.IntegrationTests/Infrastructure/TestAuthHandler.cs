using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hypesoft.IntegrationTests.Infrastructure;

/// <summary>
/// Simple test authentication handler. Reads roles from the "X-Test-Roles" header
/// (comma-separated) and issues a ClaimsPrincipal with those roles.
/// Use "X-Test-Roles: admin,user" for admin, "X-Test-Roles: user" for regular user,
/// or omit the header to simulate an unauthenticated request.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public const string RolesHeader = "X-Test-Roles";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RolesHeader, out var rolesHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var roles = rolesHeader.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.NameIdentifier, "test-user-id"),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role.Trim()));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
