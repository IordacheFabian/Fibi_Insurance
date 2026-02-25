using System;
using Domain.Models.Buildings;
using Domain.Models.Policies;

namespace Application.Reports.DTOs.Request;

public class PoliciesByBrokerReportDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }

    public PolicyStatus? PolicyStatus { get; set; }
    public string? Currency { get; set; }
    public BuildingType? BuildingType { get; set; }
}
