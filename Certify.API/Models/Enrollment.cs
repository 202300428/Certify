namespace Certify.API.Models;

public class Enrollment
{
    public int Id { get; set; }
    public string TraineeId { get; set; } = string.Empty;
    public int ScheduledSessionId { get; set; }
    public string Status { get; set; } = "Enrolled";
    public string PaymentStatus { get; set; } = "Pending";
    public string? Result { get; set; }
    public decimal EnrollmentFee { get; set; }

    public virtual ApplicationUser Trainee { get; set; } = null!;
    public virtual ScheduledSession ScheduledSession { get; set; } = null!;
}
