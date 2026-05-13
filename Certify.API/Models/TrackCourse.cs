namespace Certify.API.Models;

public class TrackCourse
{
    public int CertificationTrackId { get; set; }
    public virtual CertificationTrack CertificationTrack { get; set; } = null!;
    public int CourseId { get; set; }
    public virtual Course Course { get; set; } = null!;
}
