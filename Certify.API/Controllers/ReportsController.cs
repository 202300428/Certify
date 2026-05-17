using Certify.API.Data;
using Certify.API.Models;
using Certify.API.Models.Dto.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Controllers;

/// <summary>
/// Provides read-only operational reporting data for Training Coordinators.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "TrainingCoordinator")]
public class ReportsController(CertifyDbContext context) : ControllerBase
{
    /// <summary>Gets top-level operational totals for the reporting dashboard.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ReportSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary()
    {
        var revenue = await BuildRevenueReportAsync(null, null);

        return Ok(new ReportSummaryDto
        {
            TotalCourses = await context.Courses.CountAsync(),
            TotalSessions = await context.ScheduledSessions.CountAsync(),
            TotalEnrollments = await context.Enrollments.CountAsync(),
            ActiveEnrollments = await context.Enrollments.CountAsync(e => e.Status != "Dropped"),
            TotalCertifications = await context.Certifications.CountAsync(),
            RevenueCollected = revenue.TotalCollected,
            RevenueOutstanding = revenue.TotalOutstanding
        });
    }

    /// <summary>Gets enrollment counts, status totals, and fill rates grouped by course and category.</summary>
    [HttpGet("enrollments")]
    [ProducesResponseType(typeof(EnrollmentReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EnrollmentReportDto>> GetEnrollments([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? category)
    {
        var categories = await context.Courses
            .AsNoTracking()
            .Select(c => c.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var query = ApplySessionDateFilter(context.ScheduledSessions
            .AsNoTracking()
            .Include(s => s.Course)
            .Include(s => s.Enrollments), from, to);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(s => s.Course.Category == category);
        }

        var sessions = await query.ToListAsync();

        var rows = sessions
            .GroupBy(s => new { s.CourseId, s.Course.Title, s.Course.Category })
            .Select(g =>
            {
                var enrollments = g.SelectMany(s => s.Enrollments).ToList();
                var capacity = g.Sum(s => s.Capacity);
                var enrollmentCount = enrollments.Count;

                return new EnrollmentReportRowDto
                {
                    CourseId = g.Key.CourseId,
                    CourseTitle = g.Key.Title,
                    Category = g.Key.Category,
                    SessionCount = g.Count(),
                    TotalCapacity = capacity,
                    EnrollmentCount = enrollmentCount,
                    ConfirmedCount = CountStatus(enrollments, "Confirmed"),
                    AttendingCount = CountStatus(enrollments, "Attending"),
                    CompletedCount = CountStatus(enrollments, "Completed"),
                    DroppedCount = CountStatus(enrollments, "Dropped"),
                    PassedCount = enrollments.Count(e => IsStatus(e.Result, "Pass")),
                    FillRatePercent = Percent(enrollmentCount, capacity),
                    FeesBilled = enrollments.Where(e => !IsStatus(e.Status, "Dropped")).Sum(e => e.EnrollmentFee)
                };
            })
            .OrderByDescending(r => r.EnrollmentCount)
            .ThenBy(r => r.CourseTitle)
            .ToList();

        return Ok(new EnrollmentReportDto
        {
            From = from,
            To = to,
            Category = category,
            Categories = categories,
            Rows = rows
        });
    }

    /// <summary>Gets scheduled sessions, teaching hours, and utilization grouped by instructor.</summary>
    [HttpGet("instructor-workload")]
    [ProducesResponseType(typeof(InstructorWorkloadReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<InstructorWorkloadReportDto>> GetInstructorWorkload([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var sessions = await ApplySessionDateFilter(context.ScheduledSessions
                .AsNoTracking()
                .Include(s => s.Instructor)
                .Include(s => s.Course)
                .Include(s => s.Enrollments), from, to)
            .ToListAsync();

        var rows = sessions
            .GroupBy(s => new { s.InstructorId, s.Instructor.Name, s.Instructor.Email, s.Instructor.ExpertiseAreas })
            .Select(g =>
            {
                var capacity = g.Sum(s => s.Capacity);
                var enrollmentCount = g.SelectMany(s => s.Enrollments).Count();

                return new InstructorWorkloadRowDto
                {
                    InstructorId = g.Key.InstructorId,
                    InstructorName = g.Key.Name,
                    Email = g.Key.Email,
                    ExpertiseAreas = g.Key.ExpertiseAreas,
                    SessionCount = g.Count(),
                    TeachingHours = g.Sum(s => (decimal)(s.EndDateTime - s.StartDateTime).TotalHours),
                    EnrollmentCount = enrollmentCount,
                    TotalCapacity = capacity,
                    UtilizationPercent = Percent(enrollmentCount, capacity),
                    Courses = g.Select(s => s.Course.Title).Distinct().OrderBy(c => c).ToList()
                };
            })
            .OrderByDescending(r => r.TeachingHours)
            .ThenBy(r => r.InstructorName)
            .ToList();

        return Ok(new InstructorWorkloadReportDto
        {
            From = from,
            To = to,
            Rows = rows
        });
    }

    /// <summary>Gets certification progress grouped by certification track.</summary>
    [HttpGet("certification-completion")]
    [ProducesResponseType(typeof(CertificationCompletionReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CertificationCompletionReportDto>> GetCertificationCompletion()
    {
        var traineeRoleId = await context.Roles
            .Where(r => r.Name == "Trainee")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var totalTrainees = traineeRoleId == null
            ? 0
            : await context.UserRoles.CountAsync(ur => ur.RoleId == traineeRoleId);

        var tracks = await context.CertificationTracks
            .AsNoTracking()
            .Include(t => t.TrackCourses)
            .Include(t => t.Certifications)
            .ToListAsync();

        var rows = tracks
            .Select(t =>
            {
                var issued = CountStatus(t.Certifications, "Issued");
                var eligible = CountStatus(t.Certifications, "Eligible");
                var totalCertifications = t.Certifications.Count;

                return new CertificationCompletionRowDto
                {
                    TrackId = t.Id,
                    TrackName = t.Name,
                    RequiredCourseCount = t.TrackCourses.Count,
                    TotalCertifications = totalCertifications,
                    EligibleCount = eligible,
                    IssuedCount = issued,
                    CompletionRatePercent = Percent(totalCertifications, totalTrainees)
                };
            })
            .OrderByDescending(r => r.CompletionRatePercent)
            .ThenBy(r => r.TrackName)
            .ToList();

        return Ok(new CertificationCompletionReportDto
        {
            TotalTrainees = totalTrainees,
            Rows = rows
        });
    }

    /// <summary>Gets billed, collected, and outstanding revenue grouped by trainee.</summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RevenueReportDto>> GetRevenue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await BuildRevenueReportAsync(from, to));
    }

    private async Task<RevenueReportDto> BuildRevenueReportAsync(DateTime? from, DateTime? to)
    {
        var enrollmentQuery = context.Enrollments
            .AsNoTracking()
            .Include(e => e.Trainee)
            .Include(e => e.ScheduledSession)
            .Where(e => e.Status != "Dropped");

        if (from.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.ScheduledSession.StartDateTime >= from.Value.Date);
        }

        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1);
            enrollmentQuery = enrollmentQuery.Where(e => e.ScheduledSession.StartDateTime < end);
        }

        var paymentQuery = context.Payments
            .AsNoTracking()
            .Include(p => p.Trainee)
            .Where(p => p.Status == "Completed");

        if (from.HasValue)
        {
            paymentQuery = paymentQuery.Where(p => p.PaymentDate >= from.Value.Date);
        }

        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1);
            paymentQuery = paymentQuery.Where(p => p.PaymentDate < end);
        }

        var billings = await enrollmentQuery
            .GroupBy(e => new { e.TraineeId, e.Trainee.FullName, e.Trainee.Email })
            .Select(g => new
            {
                g.Key.TraineeId,
                g.Key.FullName,
                g.Key.Email,
                Billed = g.Sum(e => e.EnrollmentFee)
            })
            .ToListAsync();

        var payments = await paymentQuery
            .GroupBy(p => new { p.TraineeId, p.Trainee.FullName, p.Trainee.Email })
            .Select(g => new
            {
                g.Key.TraineeId,
                g.Key.FullName,
                g.Key.Email,
                Collected = g.Sum(p => p.Amount)
            })
            .ToListAsync();

        var paymentLookup = payments.ToDictionary(p => p.TraineeId);
        var billingLookup = billings.ToDictionary(b => b.TraineeId);
        var traineeIds = billingLookup.Keys.Union(paymentLookup.Keys).OrderBy(id => id).ToList();

        var rows = traineeIds
            .Select(id =>
            {
                billingLookup.TryGetValue(id, out var billing);
                paymentLookup.TryGetValue(id, out var payment);

                var billed = billing?.Billed ?? 0m;
                var collected = payment?.Collected ?? 0m;
                var outstanding = Math.Max(0m, billed - collected);
                var name = billing?.FullName ?? payment?.FullName ?? "Unknown trainee";
                var email = billing?.Email ?? payment?.Email ?? string.Empty;

                return new RevenueReportRowDto
                {
                    TraineeId = id,
                    TraineeName = name,
                    Email = email ?? string.Empty,
                    Billed = billed,
                    Collected = collected,
                    Outstanding = outstanding,
                    CollectionRatePercent = Percent(collected, billed),
                    HasOutstandingBalance = outstanding > 0
                };
            })
            .OrderByDescending(r => r.Outstanding)
            .ThenBy(r => r.TraineeName)
            .ToList();

        return new RevenueReportDto
        {
            From = from,
            To = to,
            TotalBilled = billings.Sum(b => b.Billed),
            TotalCollected = payments.Sum(p => p.Collected),
            TotalOutstanding = rows.Sum(r => r.Outstanding),
            Rows = rows
        };
    }

    private static IQueryable<ScheduledSession> ApplySessionDateFilter(IQueryable<ScheduledSession> query, DateTime? from, DateTime? to)
    {
        if (from.HasValue)
        {
            query = query.Where(s => s.StartDateTime >= from.Value.Date);
        }

        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1);
            query = query.Where(s => s.StartDateTime < end);
        }

        return query;
    }

    private static int CountStatus<T>(IEnumerable<T> items, string expected) where T : class
    {
        return items.Count(item =>
        {
            var value = item switch
            {
                Enrollment enrollment => enrollment.Status,
                Certification certification => certification.Status,
                _ => null
            };

            return IsStatus(value, expected);
        });
    }

    private static bool IsStatus(string? value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static decimal Percent(decimal part, decimal whole)
    {
        return whole <= 0 ? 0 : Math.Round(part / whole * 100, 1);
    }
}
