using Certify.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Controllers;

/// <summary>
/// Handles certification verification and lookup operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CertificationsController(CertifyDbContext context) : ControllerBase
{

    /// <summary>Publicly verifies a trainee's certification status without authentication. Intended for employer/third-party validation.</summary>
    /// <param name="traineeId">Unique identifier of the trainee.</param>
    /// <param name="certRef">Certificate reference number to verify.</param>
    /// <returns>Certification details, trainee name, track name, issue date, and completed courses.</returns>
    /// <response code="200">Certification found and verified.</response>
    /// <response code="404">Certificate reference or trainee ID not found.</response>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublicLookup([FromQuery] string traineeId, [FromQuery] string certRef)
    {
        var cert = await context.Certifications
            .Include(c => c.Trainee)
            .Include(c => c.CertificationTrack)
                .ThenInclude(ct => ct.TrackCourses)
            .Include(c => c.Trainee.Enrollments)
                .ThenInclude(e => e.ScheduledSession)
            .FirstOrDefaultAsync(c => c.TraineeId == traineeId && c.CertificateNumber == certRef);

        if (cert == null) return NotFound(new { Message = "Certification not found." });

        var completed = cert.Trainee.Enrollments
            .Where(e => e.Status == "Completed" && e.Result == "Pass")
            .Select(e => e.ScheduledSession.Course.Title)
            .ToList();

        return Ok(new
        {
            TraineeName = cert.Trainee.FullName,
            Track = cert.CertificationTrack.Name,
            Status = cert.Status,
            IssueDate = cert.IssueDate,
            CompletedCourses = completed
        });
    }
}