using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Certify.Web.Controllers;

[AllowAnonymous]
public class LookupController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    public LookupController(IHttpClientFactory f) => _httpFactory = f;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string traineeEmail, string certificateNumber)
    {
        if (string.IsNullOrWhiteSpace(traineeEmail) || string.IsNullOrWhiteSpace(certificateNumber))
        {
            ViewBag.Error = "Both fields are required.";
            return View();
        }

        var client = _httpFactory.CreateClient("CertifyApi");
        var resp = await client.GetAsync(
            $"api/certifications/public?traineeEmail={Uri.EscapeDataString(traineeEmail)}&certRef={Uri.EscapeDataString(certificateNumber)}");

        if (!resp.IsSuccessStatusCode)
        {
            ViewBag.Error = "Certificate not found. Please check the email and reference number.";
            return View();
        }

        var json = await resp.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<CertificationResult>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View("Result", result);
    }
}

public class CertificationResult
{
    public string TraineeName { get; set; } = "";
    public string Track { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public List<string> CompletedCourses { get; set; } = new();
}