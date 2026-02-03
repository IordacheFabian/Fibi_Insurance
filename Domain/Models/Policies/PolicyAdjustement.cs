using System;

namespace Domain.Models.Policies;

public enum AdjustementType
{
    BrokerCommission, 
    RiskAdjustement,
    AdminFee
}

public class PolicyAdjustement
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = default!;

    public string Name { get; set; } = default!;
    public AdjustementType AdjustementType { get; set; }
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
}
