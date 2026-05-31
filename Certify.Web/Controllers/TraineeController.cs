using Certify.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize(Roles = "Trainee")]
public class TraineeController : Controller
{
    private readonly CertifyDbContext _db;
    private readonly UserManager<API.Models.ApplicationUser> _userManager;

    public TraineeController(CertifyDbContext db, UserManager<API.Models.ApplicationUser> um)
    {
        _db = db; _userManager = um;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> Certifications()
    {
        var userId = _userManager.GetUserId(User);
        var certifications = await _db.Certifications
            .Include(c => c.CertificationTrack)
            .Where(c => c.TraineeId == userId)
            .OrderByDescending(c => c.IssueDate)
            .ToListAsync();

        var passedCourseIds = await _db.Enrollments
            .Where(e => e.TraineeId == userId && e.Status == "Completed" && e.Result == "Pass")
            .Select(e => e.ScheduledSession!.CourseId)
            .Distinct()
            .ToListAsync();

        var allTrackCourses = await _db.TrackCourses
            .Include(tc => tc.Course)
            .ToListAsync();

        var trackCourseGroups = allTrackCourses
            .GroupBy(tc => tc.CertificationTrackId)
            .ToDictionary(g => g.Key, g => g.Select(tc => tc.Course!).ToList());

        ViewBag.PassedCourseIds = passedCourseIds;
        ViewBag.TrackCourses = trackCourseGroups;

        return View(certifications);
    }
}