using System;

namespace Domain.Models.Metadatas;

public enum FeeType
{
    BrokerComission,
    RiskAdjustement,
    AdminFee
}

public class FeeConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public FeeType FeeType { get; set; }
    public decimal Percentage { get; set; }

    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; }  
}
