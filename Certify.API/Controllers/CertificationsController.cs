using Certify.API.Data;
using Certify.API.Models;
using Certify.API.Models.Dto.Certifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Certify.API.Controllers;

/// <summary>
/// Handles certification verification and lookup operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CertificationsController(CertifyDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
{

    /// <summary>Publicly verifies a trainee's certification status without authentication. Intended for employer/third-party validation.</summary>
    /// <param name="traineeEmail">Email address of the trainee.</param>
    /// <param name="certRef">Certificate reference number to verify.</param>
    /// <returns>Certification details, trainee name, track name, issue date, and completed courses.</returns>
    /// <response code="200">Certification found and verified.</response>
    /// <response code="404">Certificate reference or trainee email not found.</response>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublicLookup([FromQuery] string traineeEmail, [FromQuery] string certRef)
    {
        if (string.IsNullOrWhiteSpace(traineeEmail) || string.IsNullOrWhiteSpace(certRef))
            return BadRequest(new { Message = "Both trainee email and certificate reference are required." });

        var cert = await context.Certifications
            .Include(c => c.Trainee)
            .Include(c => c.CertificationTrack)
                .ThenInclude(ct => ct.TrackCourses)
            .Include(c => c.Trainee.Enrollments)
                .ThenInclude(e => e.ScheduledSession)
                    .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(c =>
                c.Trainee.Email == traineeEmail &&
                c.CertificateNumber == certRef);

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

    /// <summary>Gets certifications for the authenticated trainee.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Trainee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCertifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var certs = await context.Certifications
            .AsNoTracking()
            .Where(c => c.TraineeId == userId)
            .Include(c => c.CertificationTrack)
            .OrderByDescending(c => c.IssueDate)
            .ToListAsync();

        return Ok(certs);
    }

    /// <summary>Gets all certifications. Restricted to Training Coordinators.</summary>
    [HttpGet]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? trackId, [FromQuery] string? status)
    {
        var query = context.Certifications
            .AsNoTracking()
            .Include(c => c.Trainee)
            .Include(c => c.CertificationTrack)
            .AsQueryable();

        if (trackId.HasValue)
            query = query.Where(c => c.CertificationTrackId == trackId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        return Ok(await query.OrderByDescending(c => c.IssueDate).ToListAsync());
    }

    /// <summary>Issues a certification to a trainee. Restricted to Training Coordinators.</summary>
    [HttpPost]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Issue([FromBody] IssueCertificationDto dto)
    {
        var trainee = await userManager.FindByEmailAsync(dto.TraineeEmail);
        if (trainee == null)
            return BadRequest("Trainee not found.");

        var isTrainee = await userManager.IsInRoleAsync(trainee, "Trainee");
        if (!isTrainee)
            return BadRequest("User is not a trainee.");

        var track = await context.CertificationTracks.FindAsync(dto.CertificationTrackId);
        if (track == null)
            return BadRequest("Certification track not found.");

        var exists = await context.Certifications.AnyAsync(c =>
            c.TraineeId == trainee.Id && c.CertificationTrackId == dto.CertificationTrackId);

        if (exists)
            return BadRequest("Trainee already has a certification for this track.");

        var certRef = $"CERT-{track.Id:D3}-{trainee.Id[..Math.Min(8, trainee.Id.Length)]}-{DateTime.UtcNow:yyyyMMdd}";

        var cert = new Certification
        {
            TraineeId = trainee.Id,
            CertificationTrackId = dto.CertificationTrackId,
            CertificateNumber = certRef,
            IssueDate = DateTime.UtcNow,
            Status = "Eligible"
        };

        context.Certifications.Add(cert);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMyCertifications), null, cert);
    }

    /// <summary>Updates certification status (e.g., mark as Issued). Restricted to Training Coordinators.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "TrainingCoordinator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var cert = await context.Certifications.FindAsync(id);
        if (cert == null) return NotFound();

        cert.Status = status;
        await context.SaveChangesAsync();
        return Ok(cert);
    }
}