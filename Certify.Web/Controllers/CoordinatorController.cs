using Certify.API.Data;
using Certify.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Certify.Web.Controllers;

[Authorize(Roles = "TrainingCoordinator")]
public class CoordinatorController : Controller
{
    private readonly CertifyDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoordinatorController(CertifyDbContext db, UserManager<ApplicationUser> um)
    {
        _db = db; _userManager = um;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var u in users)
        {
            userRoles[u.Id] = await _userManager.GetRolesAsync(u);
        }
        ViewBag.Roles = userRoles;
        ViewBag.AllRoles = new[] { "Trainee", "Instructor", "TrainingCoordinator" };
        return View(users);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string role)
    {
        var validRoles = new[] { "Trainee", "Instructor", "TrainingCoordinator" };
        if (!validRoles.Contains(role))
        {
            TempData["Error"] = "Invalid role.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(Users));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        TempData["Success"] = $"{user.FullName} is now a {role}.";
        return RedirectToAction(nameof(Users));
    }
}