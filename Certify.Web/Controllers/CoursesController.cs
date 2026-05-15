using Certify.API.Data;
using Certify.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize(Roles = "TrainingCoordinator")]
public class CoursesController : Controller
{
    private readonly CertifyDbContext _db;
    public CoursesController(CertifyDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, string? category)
    {
        var q = _db.Courses.Include(c => c.PrerequisiteCourse).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(c => c.Title.Contains(search));
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(c => c.Category == category);

        ViewBag.Categories = await _db.Courses
            .Select(c => c.Category).Distinct().ToListAsync();
        ViewBag.Search = search;
        ViewBag.Category = category;
        return View(await q.OrderBy(c => c.Title).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses
            .Include(c => c.PrerequisiteCourse)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Prerequisites = new SelectList(await _db.Courses.ToListAsync(), "Id", "Title");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        ModelState.Remove(nameof(Course.PrerequisiteCourse));
        ModelState.Remove(nameof(Course.ScheduledSessions));
        ModelState.Remove(nameof(Course.TrackCourses));
        if (!ModelState.IsValid)
        {
            ViewBag.Prerequisites = new SelectList(await _db.Courses.ToListAsync(), "Id", "Title", course.PrerequisiteCourseId);
            return View(course);
        }
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Course '{course.Title}' created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();
        ViewBag.Prerequisites = new SelectList(
            await _db.Courses.Where(c => c.Id != id).ToListAsync(), "Id", "Title", course.PrerequisiteCourseId);
        return View(course);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course course)
    {
        if (id != course.Id) return BadRequest();
        ModelState.Remove(nameof(Course.PrerequisiteCourse));
        ModelState.Remove(nameof(Course.ScheduledSessions));
        ModelState.Remove(nameof(Course.TrackCourses));
        if (course.PrerequisiteCourseId == id)
        {
            ModelState.AddModelError(nameof(course.PrerequisiteCourseId), "A course cannot be its own prerequisite.");
        }
        if (!ModelState.IsValid)
        {
            ViewBag.Prerequisites = new SelectList(
                await _db.Courses.Where(c => c.Id != id).ToListAsync(), "Id", "Title", course.PrerequisiteCourseId);
            return View(course);
        }
        try
        {
            _db.Update(course);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Course updated.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_db.Courses.Any(c => c.Id == id)) return NotFound();
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses
            .Include(c => c.PrerequisiteCourse)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();
        // Edge case: prevent deletion if sessions exist
        var hasSessions = await _db.ScheduledSessions.AnyAsync(s => s.CourseId == id);
        if (hasSessions)
        {
            TempData["Error"] = "Cannot delete a course with scheduled sessions.";
            return RedirectToAction(nameof(Index));
        }
        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Course deleted.";
        return RedirectToAction(nameof(Index));
    }
}