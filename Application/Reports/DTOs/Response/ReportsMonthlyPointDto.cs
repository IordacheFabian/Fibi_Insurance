using System;

namespace Application.Reports.DTOs.Response;

public class ReportsMonthlyPointDto
{
    public string Month { get; set; } = default!;
    public decimal Premiums { get; set; }
    public decimal Claims { get; set; }
    public int NewPolicies { get; set; }
}