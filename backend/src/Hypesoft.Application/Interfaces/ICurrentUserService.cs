namespace Hypesoft.Application.Interfaces;

/// <summary>
/// Provides information about the currently authenticated user extracted from the JWT token.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The user's unique identifier (Keycloak subject claim).</summary>
    string? UserId { get; }

    /// <summary>The user's email address.</summary>
    string? Email { get; }

    /// <summary>The user's preferred username.</summary>
    string? Username { get; }

    /// <summary>Returns true when a valid, authenticated user is present in the current request context.</summary>
    bool IsAuthenticated { get; }
}
