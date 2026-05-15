using Certify.API.Data;
using Certify.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize(Roles = "TrainingCoordinator")]
public class SessionsController : Controller
{
    private readonly CertifyDbContext _db;
    public SessionsController(CertifyDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var sessions = await _db.ScheduledSessions
            .Include(s => s.Course)
            .Include(s => s.Instructor)
            .Include(s => s.Room)
            .OrderBy(s => s.StartDateTime)
            .ToListAsync();
        return View(sessions);
    }

    public async Task<IActionResult> Details(int id)
    {
        var session = await _db.ScheduledSessions
            .Include(s => s.Course).Include(s => s.Instructor).Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();
        return View(session);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLists();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ScheduledSession session)
    {
        await ValidateSession(session, isUpdate: false);
        ModelState.Remove(nameof(ScheduledSession.Course));
        ModelState.Remove(nameof(ScheduledSession.Instructor));
        ModelState.Remove(nameof(ScheduledSession.Room));
        ModelState.Remove(nameof(ScheduledSession.Enrollments));
        if (!ModelState.IsValid)
        {
            await PopulateLists(session);
            return View(session);
        }
        _db.ScheduledSessions.Add(session);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Session scheduled.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var session = await _db.ScheduledSessions.FindAsync(id);
        if (session == null) return NotFound();
        await PopulateLists(session);
        return View(session);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ScheduledSession session)
    {
        if (id != session.Id) return BadRequest();
        await ValidateSession(session, isUpdate: true);
        ModelState.Remove(nameof(ScheduledSession.Course));
        ModelState.Remove(nameof(ScheduledSession.Instructor));
        ModelState.Remove(nameof(ScheduledSession.Room));
        ModelState.Remove(nameof(ScheduledSession.Enrollments));
        if (!ModelState.IsValid)
        {
            await PopulateLists(session);
            return View(session);
        }
        _db.Update(session);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Session updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var session = await _db.ScheduledSessions
            .Include(s => s.Course).Include(s => s.Instructor).Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();
        return View(session);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var session = await _db.ScheduledSessions.FindAsync(id);
        if (session == null) return NotFound();
        if (await _db.Enrollments.AnyAsync(e => e.ScheduledSessionId == id))
        {
            TempData["Error"] = "Cannot delete a session with enrollments.";
            return RedirectToAction(nameof(Index));
        }
        _db.ScheduledSessions.Remove(session);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Session deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ===== HELPERS =====
    private async Task PopulateLists(ScheduledSession? s = null)
    {
        ViewBag.Courses = new SelectList(await _db.Courses.ToListAsync(), "Id", "Title", s?.CourseId);
        ViewBag.Instructors = new SelectList(await _db.Instructors.ToListAsync(), "Id", "Name", s?.InstructorId);
        ViewBag.Rooms = new SelectList(await _db.Rooms.ToListAsync(), "Id", "Name", s?.RoomId);
    }

    private async Task ValidateSession(ScheduledSession s, bool isUpdate)
    {
        if (s.EndDateTime <= s.StartDateTime)
            ModelState.AddModelError(nameof(s.EndDateTime), "End time must be after start time.");

        // Room capacity check vs session capacity
        var room = await _db.Rooms.FindAsync(s.RoomId);
        if (room != null && s.Capacity > room.Capacity)
            ModelState.AddModelError(nameof(s.Capacity), $"Capacity exceeds room capacity ({room.Capacity}).");

        // Instructor double-booking
        var instructorConflict = await _db.ScheduledSessions.AnyAsync(x =>
            x.InstructorId == s.InstructorId &&
            (!isUpdate || x.Id != s.Id) &&
            x.StartDateTime < s.EndDateTime &&
            x.EndDateTime > s.StartDateTime);
        if (instructorConflict)
            ModelState.AddModelError(nameof(s.InstructorId), "Instructor is already booked during this time.");

        // Room double-booking
        var roomConflict = await _db.ScheduledSessions.AnyAsync(x =>
            x.RoomId == s.RoomId &&
            (!isUpdate || x.Id != s.Id) &&
            x.StartDateTime < s.EndDateTime &&
            x.EndDateTime > s.StartDateTime);
        if (roomConflict)
            ModelState.AddModelError(nameof(s.RoomId), "Room is already booked during this time.");
    }
}