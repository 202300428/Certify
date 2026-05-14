using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Certify.Web.Controllers;

[Authorize(Roles = "Instructor")]
public class InstructorController : Controller
{
    public IActionResult Index() => View();
    public IActionResult MySessions() => View();
}