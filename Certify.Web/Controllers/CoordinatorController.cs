using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Certify.Web.Controllers;

[Authorize(Roles = "TrainingCoordinator")]
public class CoordinatorController : Controller
{
    public IActionResult Index() => View();
}