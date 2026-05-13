namespace Certify.API.Models;

public class ScheduledSession
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int InstructorId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int Capacity { get; set; }

    public virtual Course Course { get; set; } = null!;
    public virtual Instructor Instructor { get; set; } = null!;
    public virtual Room Room { get; set; } = null!;
    public virtual ICollection<Enrollment> Enrollments { get; set; } = [];
}