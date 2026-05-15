using Certify.API.Data;
using Certify.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly CertifyDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    public NotificationsController(CertifyDbContext db, UserManager<ApplicationUser> um) { _db = db; _userManager = um; }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var items = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = _userManager.GetUserId(User);
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (n == null) return NotFound();
        n.IsRead = true;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}