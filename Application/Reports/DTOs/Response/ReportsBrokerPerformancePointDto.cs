using System;

namespace Application.Reports.DTOs.Response;

public class ReportsBrokerPerformancePointDto
{
    public Guid BrokerId { get; set; }
    public string BrokerName { get; set; } = default!;
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public decimal WrittenPremium { get; set; }
    public decimal CollectedPremium { get; set; }
    public decimal BrokerEarnings { get; set; }
    public decimal CommissionPercentage { get; set; }
}