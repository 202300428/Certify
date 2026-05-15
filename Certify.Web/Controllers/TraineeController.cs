using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Certify.Web.Controllers;

[Authorize(Roles = "Trainee")]
public class TraineeController : Controller
{
    public IActionResult Index() => View();
}