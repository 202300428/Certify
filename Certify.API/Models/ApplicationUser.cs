using Microsoft.AspNetCore.Identity;

namespace Certify.API.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? CPR { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Notification> Notifications { get; set; } = [];
    public virtual ICollection<Enrollment> Enrollments { get; set; } = [];
    public virtual ICollection<Payment> Payments { get; set; } = [];
    public virtual ICollection<Certification> Certifications { get; set; } = [];
}
