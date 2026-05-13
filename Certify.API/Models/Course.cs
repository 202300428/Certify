namespace Certify.API.Models;

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public int Capacity { get; set; }
    public int DurationHours { get; set; }

    public int? PrerequisiteCourseId { get; set; }

    public virtual Course? PrerequisiteCourse { get; set; }
    public virtual ICollection<ScheduledSession> ScheduledSessions { get; set; } = [];
    public virtual ICollection<TrackCourse> TrackCourses { get; set; } = [];
}
