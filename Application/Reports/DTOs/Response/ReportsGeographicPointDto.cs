using System;

namespace Application.Reports.DTOs.Response;

public class ReportsGeographicPointDto
{
    public string Region { get; set; } = default!;
    public int Policies { get; set; }
    public decimal Premium { get; set; }
    public int Claims { get; set; }
}