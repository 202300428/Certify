namespace Certify.API.Models;

public class Payment
{
    public int Id { get; set; }
    public string TraineeId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Completed";
    public string? Description { get; set; }

    public virtual ApplicationUser Trainee { get; set; } = null!;
}
