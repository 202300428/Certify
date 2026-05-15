using Certify.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize(Roles = "Trainee")]
public class CatalogController : Controller
{
    private readonly CertifyDbContext _db;
    public CatalogController(CertifyDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, string? category)
    {
        var q = _db.ScheduledSessions
            .Include(s => s.Course)
            .Include(s => s.Instructor)
            .Include(s => s.Room)
            .Where(s => s.StartDateTime > DateTime.UtcNow)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(s => s.Course.Title.Contains(search));
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(s => s.Course.Category == category);

        ViewBag.Categories = await _db.Courses.Select(c => c.Category).Distinct().ToListAsync();
        ViewBag.Search = search;
        ViewBag.Category = category;
        return View(await q.OrderBy(s => s.StartDateTime).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var session = await _db.ScheduledSessions
            .Include(s => s.Course).ThenInclude(c => c!.PrerequisiteCourse)
            .Include(s => s.Instructor)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();
        ViewBag.EnrolledCount = await _db.Enrollments
            .CountAsync(e => e.ScheduledSessionId == id && e.Status != "Dropped");
        return View(session);
    }
}