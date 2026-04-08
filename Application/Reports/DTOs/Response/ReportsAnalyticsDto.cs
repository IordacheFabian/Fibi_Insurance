using System;

namespace Application.Reports.DTOs.Response;

public class ReportsAnalyticsDto
{
    public string CurrencyCode { get; set; } = default!;
    public string CurrencyName { get; set; } = default!;
    public ReportsSummaryDto Summary { get; set; } = new();
    public List<ReportsMonthlyPointDto> Monthly { get; set; } = new();
    public List<ReportsGeographicPointDto> Geographic { get; set; } = new();
    public List<ReportsClaimsBreakdownPointDto> ClaimsBreakdown { get; set; } = new();
    public List<ReportsBrokerPerformancePointDto> BrokerPerformance { get; set; } = new();
}