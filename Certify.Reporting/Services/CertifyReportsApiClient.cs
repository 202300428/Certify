using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Certify.Reporting.Models.Auth;
using Certify.Reporting.Models.Reports;

namespace Certify.Reporting.Services;

public class CertifyReportsApiClient(HttpClient httpClient) : ICertifyReportsApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ApiAuthResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ApiAuthResponse>(JsonOptions, cancellationToken);
    }

    public Task<ReportSummaryModel> GetSummaryAsync(string token, CancellationToken cancellationToken)
    {
        return GetAsync<ReportSummaryModel>("api/reports/summary", token, cancellationToken);
    }

    public Task<EnrollmentReportModel> GetEnrollmentReportAsync(string token, DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken)
    {
        return GetAsync<EnrollmentReportModel>(BuildPath("api/reports/enrollments", from, to, category), token, cancellationToken);
    }

    public Task<InstructorWorkloadReportModel> GetInstructorWorkloadAsync(string token, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        return GetAsync<InstructorWorkloadReportModel>(BuildPath("api/reports/instructor-workload", from, to), token, cancellationToken);
    }

    public Task<CertificationCompletionReportModel> GetCertificationCompletionAsync(string token, CancellationToken cancellationToken)
    {
        return GetAsync<CertificationCompletionReportModel>("api/reports/certification-completion", token, cancellationToken);
    }

    public Task<RevenueReportModel> GetRevenueReportAsync(string token, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        return GetAsync<RevenueReportModel>(BuildPath("api/reports/revenue", from, to), token, cancellationToken);
    }

    private async Task<T> GetAsync<T>(string path, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return payload ?? throw new ApiClientException(response.StatusCode, "The API returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = response.ReasonPhrase ?? "Request failed.";
        }

        throw new ApiClientException(response.StatusCode, message);
    }

    private static string BuildPath(string route, DateTime? from, DateTime? to, string? category = null)
    {
        var query = new List<string>();

        if (from.HasValue)
        {
            query.Add("from=" + Uri.EscapeDataString(FormatDate(from.Value)));
        }

        if (to.HasValue)
        {
            query.Add("to=" + Uri.EscapeDataString(FormatDate(to.Value)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query.Add("category=" + Uri.EscapeDataString(category));
        }

        return query.Count == 0 ? route : $"{route}?{string.Join("&", query)}";
    }

    private static string FormatDate(DateTime value)
    {
        return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
