namespace Certify.API.Models;

public class Certification
{
    public int Id { get; set; }
    public string TraineeId { get; set; } = string.Empty;
    public int CertificationTrackId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string Status { get; set; } = "Eligible";

    public virtual ApplicationUser Trainee { get; set; } = null!;
    public virtual CertificationTrack CertificationTrack { get; set; } = null!;
}
