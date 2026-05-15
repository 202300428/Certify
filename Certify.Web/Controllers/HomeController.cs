using System.Diagnostics;
using Certify.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Certify.Web.Controllers;

public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (User.Identity!.IsAuthenticated)
        {
            if (User.IsInRole("TrainingCoordinator")) return RedirectToAction("Index", "Coordinator");
            if (User.IsInRole("Instructor")) return RedirectToAction("Index", "Instructor");
            if (User.IsInRole("Trainee")) return RedirectToAction("Index", "Trainee");
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}