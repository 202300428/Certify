namespace Certify.API.Models.Dto.Auth;

/// <summary>
/// Contains credentials and profile information for registering a new system user.
/// </summary>
public class RegisterDto
{
    /// <summary>Valid email address to be used as the username.</summary>
    /// <example>trainee@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Secure password meeting Identity complexity requirements.</summary>
    /// <example>P@ssw0rd123!</example>
    public string Password { get; set; } = string.Empty;

    /// <summary>System role to assign. Valid values: Trainee, Instructor, TrainingCoordinator.</summary>
    /// <example>Trainee</example>
    public string Role { get; set; } = string.Empty;

    /// <summary>Optional full name for display and certification purposes.</summary>
    /// <example>Ahmed Ali</example>
    public string? FullName { get; set; }
}