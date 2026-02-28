using Hypesoft.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Hypesoft.Infrastructure.Services;

/// <summary>
/// Reads current user information from the HTTP context claims principal.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User?.FindFirstValue("sub");

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email) ??
        User?.FindFirstValue("email");

    public string? Username =>
        User?.FindFirstValue("preferred_username") ??
        User?.FindFirstValue(ClaimTypes.Name);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
