using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace Hypesoft.API.Authentication;

public sealed class KeycloakClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity!;

        // Handle flat "roles" claim added by the Keycloak protocol mapper.
        // The mapper may emit multiple individual claims or a single JSON array string.
        var rolesClaims = identity.FindAll("roles").ToList();
        foreach (var rolesClaim in rolesClaims)
        {
            if (rolesClaim.Value.StartsWith('['))
            {
                var roles = JsonSerializer.Deserialize<string[]>(rolesClaim.Value) ?? [];
                foreach (var role in roles)
                {
                    if (!identity.HasClaim(ClaimTypes.Role, role))
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
            else
            {
                if (!identity.HasClaim(ClaimTypes.Role, rolesClaim.Value))
                    identity.AddClaim(new Claim(ClaimTypes.Role, rolesClaim.Value));
            }
        }

        // Fallback: handle nested realm_access.roles structure (no protocol mapper configured).
        var realmAccessClaim = identity.FindFirst("realm_access");
        if (realmAccessClaim != null)
        {
            try
            {
                var realmAccess = JsonSerializer.Deserialize<JsonElement>(realmAccessClaim.Value);
                if (realmAccess.TryGetProperty("roles", out var rolesElement))
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (roleName != null && !identity.HasClaim(ClaimTypes.Role, roleName))
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
            }
            catch (JsonException) { }
        }

        return Task.FromResult(principal);
    }
}
