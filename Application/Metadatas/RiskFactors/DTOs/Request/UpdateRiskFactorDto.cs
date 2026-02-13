using System;
using Domain.Models.Metadatas;

namespace Application.Metadatas.RiskFactors.DTOs.Request;

public class UpdateRiskFactorDto
{
    public string Name { get; set; } = null!;
    public RiskLevel RiskLevel { get; set; }
    public decimal AdjustementPercentage { get; set; }
    public bool IsActive { get; set; }
}
