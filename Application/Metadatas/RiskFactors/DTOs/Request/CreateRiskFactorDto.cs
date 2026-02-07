using System;
using Domain.Models.Metadatas;

namespace Application.Metadatas.RiskFactors.DTOs.Request;

public class CreateRiskFactorDto
{
    public RiskLevel Level { get; set; }
    public Guid ReferenceId { get; set; }
    public decimal AdjustementPercentage { get; set; }
    public bool IsActive { get; set; } = true;
}
