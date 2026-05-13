namespace Certify.API.Models.Dto.Auth;

/// <summary>
/// Response payload containing the authenticated session token and user context.
/// </summary>
public class AuthResponseDto
{
    /// <summary>JWT bearer token for subsequent authorized requests.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Authenticated user's email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Primary role assigned to the user.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>UTC expiration timestamp of the JWT.</summary>
    public DateTime ExpiresAt { get; set; }
}