namespace Certify.API.Models.Dto.Payments;

public class CreatePaymentDto
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
