namespace Certify.API.Models;

public class Instructor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ExpertiseAreas { get; set; }

    public virtual ICollection<ScheduledSession> ScheduledSessions { get; set; } = [];
}
