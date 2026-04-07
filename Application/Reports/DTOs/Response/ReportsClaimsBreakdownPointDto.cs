using System;

namespace Application.Reports.DTOs.Response;

public class ReportsClaimsBreakdownPointDto
{
    public string Name { get; set; } = default!;
    public int Value { get; set; }
}