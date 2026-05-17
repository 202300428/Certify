using Certify.Reporting.Infrastructure;
using Certify.Reporting.Models.Auth;
using Certify.Reporting.Services;
using Microsoft.AspNetCore.Mvc;

namespace Certify.Reporting.Controllers;

public class AccountController(ICertifyReportsApiClient apiClient) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HasValidCoordinatorSession())
        {
            return RedirectToLocalReport(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ApiAuthResponse? auth;
        try
        {
            auth = await apiClient.LoginAsync(new ApiLoginRequest
            {
                Email = model.Email,
                Password = model.Password
            }, cancellationToken);
        }
        catch (ApiClientException)
        {
            ModelState.AddModelError(string.Empty, "The reporting API is unavailable. Confirm the API project is running.");
            return View(model);
        }

        if (auth == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!string.Equals(auth.Role, "TrainingCoordinator", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Only Training Coordinators can access the reporting application.");
            return View(model);
        }

        HttpContext.Session.SetString(ReportingSessionKeys.Token, auth.Token);
        HttpContext.Session.SetString(ReportingSessionKeys.Role, auth.Role);
        HttpContext.Session.SetString(ReportingSessionKeys.Email, auth.Email);
        HttpContext.Session.SetString(ReportingSessionKeys.ExpiresAt, auth.ExpiresAt.ToUniversalTime().ToString("O"));

        return RedirectToLocalReport(model.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    private bool HasValidCoordinatorSession()
    {
        var token = HttpContext.Session.GetString(ReportingSessionKeys.Token);
        var role = HttpContext.Session.GetString(ReportingSessionKeys.Role);
        var expiresAtValue = HttpContext.Session.GetString(ReportingSessionKeys.ExpiresAt);

        return !string.IsNullOrWhiteSpace(token)
            && string.Equals(role, "TrainingCoordinator", StringComparison.OrdinalIgnoreCase)
            && DateTime.TryParse(expiresAtValue, out var expiresAt)
            && expiresAt.ToUniversalTime() > DateTime.UtcNow;
    }

    private IActionResult RedirectToLocalReport(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Reports");
    }
}
