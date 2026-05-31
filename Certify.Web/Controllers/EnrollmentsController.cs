using Certify.API.Data;
using Certify.API.Models;
using Certify.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize(Roles = "Trainee")]
public class EnrollmentsController : Controller
{
    private readonly CertifyDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<CourseHub> _hub;

    public EnrollmentsController(CertifyDbContext db, UserManager<ApplicationUser> um, IHubContext<CourseHub> hub)
    {
        _db = db; _userManager = um; _hub = hub;
    }

    public async Task<IActionResult> My()
    {
        var userId = _userManager.GetUserId(User);
        var enrollments = await _db.Enrollments
            .Include(e => e.ScheduledSession).ThenInclude(s => s!.Course)
            .Include(e => e.ScheduledSession).ThenInclude(s => s!.Instructor)
            .Where(e => e.TraineeId == userId)
            .OrderByDescending(e => e.Id)
            .ToListAsync();
        return View(enrollments);
    }

    public async Task<IActionResult> Upcoming()
    {
        var userId = _userManager.GetUserId(User);
        var enrollments = await _db.Enrollments
            .Include(e => e.ScheduledSession).ThenInclude(s => s!.Course)
            .Include(e => e.ScheduledSession).ThenInclude(s => s!.Instructor)
            .Include(e => e.ScheduledSession).ThenInclude(s => s!.Room)
            .Where(e => e.TraineeId == userId
                && e.Status != "Dropped"
                && e.Status != "Completed"
                && e.ScheduledSession!.StartDateTime > DateTime.UtcNow)
            .OrderBy(e => e.ScheduledSession!.StartDateTime)
            .ToListAsync();
        return View(enrollments);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int sessionId)
    {
        var userId = _userManager.GetUserId(User)!;
        var session = await _db.ScheduledSessions
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null) return NotFound();

        // 1. Look for an existing enrollment row (active OR dropped)
        var existing = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.TraineeId == userId && e.ScheduledSessionId == sessionId);

        if (existing != null && existing.Status != "Dropped")
        {
            TempData["Error"] = "You are already enrolled in this session.";
            return RedirectToAction("Details", "Catalog", new { id = sessionId });
        }

        // 2. Capacity check
        var enrolledCount = await _db.Enrollments.CountAsync(e => e.ScheduledSessionId == sessionId && e.Status != "Dropped");
        if (enrolledCount >= session.Capacity)
        {
            TempData["Error"] = "Session is full.";
            return RedirectToAction("Details", "Catalog", new { id = sessionId });
        }

        // 3. Prerequisite check
        if (session.Course?.PrerequisiteCourseId != null)
        {
            var hasPrereq = await _db.Enrollments.AnyAsync(e =>
                e.TraineeId == userId &&
                e.Status == "Completed" && e.Result == "Pass" &&
                e.ScheduledSession!.CourseId == session.Course.PrerequisiteCourseId);
            if (!hasPrereq)
            {
                TempData["Error"] = "You must complete the prerequisite course first.";
                return RedirectToAction("Details", "Catalog", new { id = sessionId });
            }
        }

        // 4. Create OR revive the enrollment
        if (existing != null)
        {
            // Revive the dropped row instead of inserting a new one
            existing.Status = "Enrolled";
            existing.PaymentStatus = "Pending";
            existing.EnrollmentFee = session.Course?.Fee ?? 0m;
            existing.Result = null;
        }
        else
        {
            _db.Enrollments.Add(new Enrollment
            {
                TraineeId = userId,
                ScheduledSessionId = sessionId,
                Status = "Enrolled",
                PaymentStatus = "Pending",
                EnrollmentFee = session.Course?.Fee ?? 0m
            });
        }

        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = "Enrollment Confirmed",
            Message = $"You are enrolled in {session.Course?.Title}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            EntityType = "Enrollment",
            EntityId = sessionId
        });

        await _db.SaveChangesAsync();

        // 5. SignalR broadcast
        var newCount = enrolledCount + 1;
        await _hub.Clients.All.SendAsync("EnrollmentCountChanged", sessionId, newCount);

        TempData["Success"] = $"Enrolled in {session.Course?.Title}.";
        return RedirectToAction(nameof(My));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Drop(int id)
    {
        var userId = _userManager.GetUserId(User);
        var enrollment = await _db.Enrollments.FirstOrDefaultAsync(e => e.Id == id && e.TraineeId == userId);
        if (enrollment == null) return NotFound();
        if (enrollment.Status == "Completed")
        {
            TempData["Error"] = "Cannot drop a completed enrollment.";
            return RedirectToAction(nameof(My));
        }
        enrollment.Status = "Dropped";
        await _db.SaveChangesAsync();

        var newCount = await _db.Enrollments.CountAsync(e => e.ScheduledSessionId == enrollment.ScheduledSessionId && e.Status != "Dropped");
        await _hub.Clients.All.SendAsync("EnrollmentCountChanged", enrollment.ScheduledSessionId, newCount);

        TempData["Success"] = "Enrollment dropped.";
        return RedirectToAction(nameof(My));
    }
}