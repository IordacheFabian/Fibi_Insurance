using System;
using Application.Buildings.DTOs.Request;
using Domain.Models.Buildings;
using Domain.Models.Metadatas;

namespace Application.Metadatas.RiskFactors.DTOs.Request;

public class CreateRiskFactorDto
{
    public string Name { get; set; } = null!;
    public RiskLevel Level { get; set; }
    public Guid ReferenceId { get; set; }
    public BuildingType BuildingType { get; set; }
    public decimal AdjustementPercentage { get; set; }
    public bool IsActive { get; set; } = true;
}
