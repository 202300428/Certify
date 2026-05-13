using Certify.API.Data;
using Certify.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Certify.API.Controllers;

/// <summary>
/// Manages trainee enrollment lifecycle and capacity validation.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EnrollmentsController(CertifyDbContext context) : ControllerBase
{

    /// <summary>Enrolls the authenticated trainee into a scheduled session. Validates capacity limits.</summary>
    /// <param name="sessionId">ID of the target scheduled session.</param>
    /// <returns>Created enrollment record with initial status and pending payment.</returns>
    /// <response code="201">Enrollment created successfully.</response>
    /// <response code="400">Session is full or invalid request.</response>
    /// <response code="403">User lacks Trainee role.</response>
    [HttpPost]
    [Authorize(Roles = "Trainee")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Enroll([FromBody] int sessionId)
    {
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        var session = await context.ScheduledSessions
            .Include(s => s.Course)
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return NotFound();
        if (session.Enrollments.Count >= session.Capacity) return BadRequest("Session is full.");

        var enrollment = new Enrollment
        {
            TraineeId = userId,
            ScheduledSessionId = sessionId,
            EnrollmentFee = session.Course.Fee,
            Status = "Enrolled",
            PaymentStatus = "Pending"
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMyEnrollments), new { }, enrollment);
    }

    /// <summary>Retrieves all enrollment records for the currently authenticated trainee.</summary>
    /// <returns>List of enrollments with course titles, dates, and status.</returns>
    /// <response code="200">Enrollment history retrieved.</response>
    [HttpGet("my")]
    [Authorize(Roles = "Trainee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        var enrollments = await context.Enrollments
            .Where(e => e.TraineeId == userId)
            .Include(e => e.ScheduledSession)
            .ThenInclude(ss => ss.Course)
            .ToListAsync();
        return Ok(enrollments);
    }
}