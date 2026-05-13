namespace Certify.API.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Equipment { get; set; }

    public virtual ICollection<ScheduledSession> ScheduledSessions { get; set; } = [];
}
