using System;

namespace Application.Metadatas.RiskFactors.DTOs.Request;

public class UpdateRiskFactorDto
{
    public decimal AdjustementPercentage { get; set; }
    public bool IsActive { get; set; }
}
