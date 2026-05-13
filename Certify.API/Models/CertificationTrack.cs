namespace Certify.API.Models;

public class CertificationTrack
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public virtual ICollection<TrackCourse> TrackCourses { get; set; } = [];
    public virtual ICollection<Certification> Certifications { get; set; } = [];
}
