using System;

namespace Application.Reports.DTOs.Request;

public class ReportsAnalyticsRequestDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? Currency { get; set; }
    public bool FilterByCurrency { get; set; }
}