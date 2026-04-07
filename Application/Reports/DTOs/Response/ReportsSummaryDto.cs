using System;

namespace Application.Reports.DTOs.Response;

public class ReportsSummaryDto
{
    public decimal TotalPremiumRevenue { get; set; }
    public decimal ClaimsRatio { get; set; }
    public decimal PortfolioGrowth { get; set; }
    public decimal CollectionRate { get; set; }
    public int TotalPolicies { get; set; }
    public decimal TotalClaimsIncurred { get; set; }
}