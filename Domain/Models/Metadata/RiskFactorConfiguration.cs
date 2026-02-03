using System;

namespace Domain.Models.Metadata;

public enum RiskLevel
{
    Country,
    County,
    City,
    BuildingType
}

public enum BuildingType
{
    Residential,
    Commercial,
    Industrial,
    MixedUse
}

public class RiskFactorConfiguration
{
    public Guid Id { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public Guid? ReferenceID { get; set; }
    public BuildingType? BuildingType { get; set; }
    public decimal AdjustementPercentage { get; set; }  
    public bool IsActive { get; set; }
}
