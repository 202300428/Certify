using System.ComponentModel.DataAnnotations;

namespace Certify.API.Models;

public class Instructor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
    public string? ExpertiseAreas { get; set; }

    public virtual ICollection<ScheduledSession> ScheduledSessions { get; set; } = [];
}
