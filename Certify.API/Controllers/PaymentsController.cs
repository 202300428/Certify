using Certify.API.Data;
using Certify.API.Models;
using Certify.API.Models.Dto.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Certify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentsController(CertifyDbContext context) : ControllerBase
{
    [HttpGet("my")]
    [Authorize(Roles = "Trainee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPayments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var payments = await context.Payments
            .AsNoTracking()
            .Where(p => p.TraineeId == userId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return Ok(payments);
    }

    [HttpGet]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? traineeId)
    {
        var query = context.Payments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(traineeId))
            query = query.Where(p => p.TraineeId == traineeId);

        var payments = await query
            .Include(p => p.Trainee)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return Ok(payments);
    }

    [HttpPost]
    [Authorize(Roles = "Trainee")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var payment = new Payment
        {
            TraineeId = userId!,
            Amount = dto.Amount,
            Description = dto.Description,
            PaymentDate = DateTime.UtcNow,
            Status = "Completed"
        };

        context.Payments.Add(payment);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMyPayments), null, payment);
    }
}
