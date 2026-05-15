using Certify.API.Data;
using Certify.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize(Roles = "Instructor")]
public class InstructorController : Controller
{
    private readonly CertifyDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public InstructorController(CertifyDbContext db, UserManager<ApplicationUser> um)
    {
        _db = db; _userManager = um;
    }

    public IActionResult Index() => RedirectToAction(nameof(MySessions));

    public async Task<IActionResult> MySessions()
    {
        var user = await _userManager.GetUserAsync(User);
        var instructor = await _db.Instructors.FirstOrDefaultAsync(i => i.Email == user!.Email);
        if (instructor == null) return View(new List<ScheduledSession>());

        var sessions = await _db.ScheduledSessions
            .Include(s => s.Course)
            .Include(s => s.Room)
            .Where(s => s.InstructorId == instructor.Id)
            .OrderBy(s => s.StartDateTime)
            .ToListAsync();
        return View(sessions);
    }

    public async Task<IActionResult> Roster(int sessionId)
    {
        var session = await _db.ScheduledSessions
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null) return NotFound();

        var enrollments = await _db.Enrollments
            .Include(e => e.Trainee)
            .Where(e => e.ScheduledSessionId == sessionId && e.Status != "Dropped")
            .ToListAsync();

        ViewBag.Session = session;
        return View(enrollments);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordResult(int enrollmentId, string result)
    {
        var enrollment = await _db.Enrollments
            .Include(e => e.ScheduledSession).ThenInclude(s => s!.Course)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);
        if (enrollment == null) return NotFound();
        if (result != "Pass" && result != "Fail")
        {
            TempData["Error"] = "Invalid result.";
            return RedirectToAction(nameof(Roster), new { sessionId = enrollment.ScheduledSessionId });
        }

        enrollment.Result = result;
        enrollment.Status = "Completed";

        _db.Notifications.Add(new Notification
        {
            UserId = enrollment.TraineeId,
            Title = "Assessment Recorded",
            Message = $"Your result for {enrollment.ScheduledSession?.Course?.Title}: {result}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            EntityType = "Assessment",
            EntityId = enrollment.Id
        });

        await _db.SaveChangesAsync();

        // After recording a Pass, check if trainee is now eligible for any track
        if (result == "Pass") await CheckCertificationEligibility(enrollment.TraineeId);

        TempData["Success"] = $"Recorded {result} for trainee.";
        return RedirectToAction(nameof(Roster), new { sessionId = enrollment.ScheduledSessionId });
    }

    private async Task CheckCertificationEligibility(string traineeId)
    {
        var tracks = await _db.CertificationTracks.Include(t => t.TrackCourses).ToListAsync();
        foreach (var track in tracks)
        {
            // Already certified?
            if (await _db.Certifications.AnyAsync(c => c.TraineeId == traineeId && c.CertificationTrackId == track.Id))
                continue;

            var requiredCourseIds = track.TrackCourses.Select(tc => tc.CourseId).ToList();
            if (requiredCourseIds.Count == 0) continue; // skip tracks with no required courses

            var passedCourseIds = await _db.Enrollments
                .Where(e => e.TraineeId == traineeId && e.Status == "Completed" && e.Result == "Pass")
                .Select(e => e.ScheduledSession!.CourseId)
                .Distinct()
                .ToListAsync();

            if (requiredCourseIds.All(id => passedCourseIds.Contains(id)))
            {
                var certNumber = $"CERT-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

                // 1. Create and save the certification first so it gets an Id
                var cert = new Certification
                {
                    TraineeId = traineeId,
                    CertificationTrackId = track.Id,
                    CertificateNumber = certNumber,
                    IssueDate = DateTime.UtcNow,
                    Status = "Eligible"
                };
                _db.Certifications.Add(cert);
                await _db.SaveChangesAsync();

                // 2. Now create the notification with a proper reference to the saved cert
                _db.Notifications.Add(new Notification
                {
                    UserId = traineeId,
                    Title = "Certification Earned",
                    Message = $"You are now certified: {track.Name} (#{certNumber})",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    EntityType = "Certification",
                    EntityId = cert.Id
                });
                await _db.SaveChangesAsync();
            }
        }
    }
}