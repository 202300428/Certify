namespace Certify.API.Models.Dto.Auth;

/// <summary>
/// Credentials required to authenticate and obtain a JWT token.
/// </summary>
public class LoginDto
{
    /// <summary>Registered email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Account password.</summary>
    public string Password { get; set; } = string.Empty;
}