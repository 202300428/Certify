using System.Net;
using Certify.Reporting.Infrastructure;
using Certify.Reporting.Models.Reports;
using Certify.Reporting.Services;
using Microsoft.AspNetCore.Mvc;

namespace Certify.Reporting.Controllers;

public class ReportsController(ICertifyReportsApiClient apiClient) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var token = GetTokenOrNull();
        if (token == null) return RedirectToLogin();

        return await LoadReportAsync(
            async ct => View(new ReportDashboardViewModel
            {
                Summary = await apiClient.GetSummaryAsync(token, ct)
            }),
            cancellationToken);
    }

    public async Task<IActionResult> Enrollments(DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken)
    {
        var token = GetTokenOrNull();
        if (token == null) return RedirectToLogin();

        return await LoadReportAsync(
            async ct => View(await apiClient.GetEnrollmentReportAsync(token, from, to, category, ct)),
            cancellationToken);
    }

    public async Task<IActionResult> InstructorWorkload(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var token = GetTokenOrNull();
        if (token == null) return RedirectToLogin();

        return await LoadReportAsync(
            async ct => View(await apiClient.GetInstructorWorkloadAsync(token, from, to, ct)),
            cancellationToken);
    }

    public async Task<IActionResult> CertificationCompletion(CancellationToken cancellationToken)
    {
        var token = GetTokenOrNull();
        if (token == null) return RedirectToLogin();

        return await LoadReportAsync(
            async ct => View(await apiClient.GetCertificationCompletionAsync(token, ct)),
            cancellationToken);
    }

    public async Task<IActionResult> Revenue(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var token = GetTokenOrNull();
        if (token == null) return RedirectToLogin();

        return await LoadReportAsync(
            async ct => View(await apiClient.GetRevenueReportAsync(token, from, to, ct)),
            cancellationToken);
    }

    private async Task<IActionResult> LoadReportAsync(Func<CancellationToken, Task<IActionResult>> loadReport, CancellationToken cancellationToken)
    {
        try
        {
            return await loadReport(cancellationToken);
        }
        catch (ApiClientException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Your reporting session is not authorized. Please sign in again.";
            return RedirectToLogin();
        }
        catch (ApiClientException ex)
        {
            return View("ReportError", new ReportErrorViewModel
            {
                StatusCode = (int)ex.StatusCode,
                Message = ex.Message
            });
        }
        catch (HttpRequestException)
        {
            return View("ReportError", new ReportErrorViewModel
            {
                Message = "The reporting API could not be reached. Confirm the API project is running."
            });
        }
    }

    private string? GetTokenOrNull()
    {
        var token = HttpContext.Session.GetString(ReportingSessionKeys.Token);
        var role = HttpContext.Session.GetString(ReportingSessionKeys.Role);
        var expiresAtValue = HttpContext.Session.GetString(ReportingSessionKeys.ExpiresAt);

        if (string.IsNullOrWhiteSpace(token)
            || !string.Equals(role, "TrainingCoordinator", StringComparison.OrdinalIgnoreCase)
            || !DateTime.TryParse(expiresAtValue, out var expiresAt)
            || expiresAt.ToUniversalTime() <= DateTime.UtcNow)
        {
            HttpContext.Session.Clear();
            return null;
        }

        return token;
    }

    private IActionResult RedirectToLogin()
    {
        return RedirectToAction("Login", "Account", new { returnUrl = $"{Request.Path}{Request.QueryString}" });
    }
}
