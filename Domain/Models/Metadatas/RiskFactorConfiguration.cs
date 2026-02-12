using System;
using Domain.Models.Buildings;

namespace Domain.Models.Metadatas;

public enum RiskLevel
{
    Country,
    County,
    City,
    BuildingType
}


public class RiskFactorConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public RiskLevel RiskLevel { get; set; }
    public Guid? ReferenceID { get; set; }
    public BuildingType? BuildingType { get; set; }
    public decimal AdjustementPercentage { get; set; }
    public bool IsActive { get; set; }
}
