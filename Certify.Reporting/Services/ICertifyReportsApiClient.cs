using Certify.Reporting.Models.Auth;
using Certify.Reporting.Models.Reports;

namespace Certify.Reporting.Services;

public interface ICertifyReportsApiClient
{
    Task<ApiAuthResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken);
    Task<ReportSummaryModel> GetSummaryAsync(string token, CancellationToken cancellationToken);
    Task<EnrollmentReportModel> GetEnrollmentReportAsync(string token, DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken);
    Task<InstructorWorkloadReportModel> GetInstructorWorkloadAsync(string token, DateTime? from, DateTime? to, CancellationToken cancellationToken);
    Task<CertificationCompletionReportModel> GetCertificationCompletionAsync(string token, CancellationToken cancellationToken);
    Task<RevenueReportModel> GetRevenueReportAsync(string token, DateTime? from, DateTime? to, CancellationToken cancellationToken);
}
