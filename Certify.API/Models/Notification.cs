namespace Certify.API.Models;

public class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public string Title { get; set; }
    public string Message { get; set; }

    public string EntityType { get; set; }
    public int? EntityId { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    public bool SendEmail { get; set; }
    public bool SendSms { get; set; }
}
