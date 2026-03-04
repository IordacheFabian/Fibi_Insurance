using System;

namespace Domain.Models.Policies;

public enum AdjustmentType
{
    BrokerCommission, 
    RiskAdjustment,
    AdminFee
}

public class PolicyAdjustment
{
    public Guid Id { get; set; }
    public Guid PolicyVersionId { get; set; }
    public PolicyVersion PolicyVersion { get; set; } = default!;

    public string Name { get; set; } = default!;
    public AdjustmentType AdjustmentType { get; set; }
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
}
