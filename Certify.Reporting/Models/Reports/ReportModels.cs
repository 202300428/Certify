namespace Certify.Reporting.Models.Reports;

public class ReportDashboardViewModel
{
    public ReportSummaryModel Summary { get; set; } = new();
}

public class ReportSummaryModel
{
    public int TotalCourses { get; set; }
    public int TotalSessions { get; set; }
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int TotalCertifications { get; set; }
    public decimal RevenueCollected { get; set; }
    public decimal RevenueOutstanding { get; set; }
}

public class EnrollmentReportModel
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Category { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<EnrollmentReportRowModel> Rows { get; set; } = [];
}

public class EnrollmentReportRowModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public int TotalCapacity { get; set; }
    public int EnrollmentCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int AttendingCount { get; set; }
    public int CompletedCount { get; set; }
    public int DroppedCount { get; set; }
    public int PassedCount { get; set; }
    public decimal FillRatePercent { get; set; }
    public decimal FeesBilled { get; set; }
}

public class InstructorWorkloadReportModel
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public List<InstructorWorkloadRowModel> Rows { get; set; } = [];
}

public class InstructorWorkloadRowModel
{
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ExpertiseAreas { get; set; }
    public int SessionCount { get; set; }
    public decimal TeachingHours { get; set; }
    public int EnrollmentCount { get; set; }
    public int TotalCapacity { get; set; }
    public decimal UtilizationPercent { get; set; }
    public List<string> Courses { get; set; } = [];
}

public class CertificationCompletionReportModel
{
    public int TotalTrainees { get; set; }
    public List<CertificationCompletionRowModel> Rows { get; set; } = [];
}

public class CertificationCompletionRowModel
{
    public int TrackId { get; set; }
    public string TrackName { get; set; } = string.Empty;
    public int RequiredCourseCount { get; set; }
    public int TotalCertifications { get; set; }
    public int EligibleCount { get; set; }
    public int IssuedCount { get; set; }
    public decimal CompletionRatePercent { get; set; }
}

public class RevenueReportModel
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
    public List<RevenueReportRowModel> Rows { get; set; } = [];
}

public class RevenueReportRowModel
{
    public string TraineeId { get; set; } = string.Empty;
    public string TraineeName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Billed { get; set; }
    public decimal Collected { get; set; }
    public decimal Outstanding { get; set; }
    public decimal CollectionRatePercent { get; set; }
    public bool HasOutstandingBalance { get; set; }
}

public class ReportErrorViewModel
{
    public string Title { get; set; } = "Report unavailable";
    public string Message { get; set; } = "The reporting API could not return the requested data.";
    public int? StatusCode { get; set; }
}
