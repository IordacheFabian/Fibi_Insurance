using System;

namespace Domain.Models.Policies;

public enum EndorsementType
{
    InsuredValueChange,
    PeriosExtension,
    RiskUpdate,
    ManualAdjustement
}

public class PolicyEndorsement
{
    public Guid Id {get; set; }
    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = default!;
    public EndorsementType EndorsementType { get; set; }
    public string Reason { get; set; } = default!;
    public DateOnly EffectiveDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = default!;

    public int OldVersionNumber { get; set; }
    public int NewVersionNumber { get; set; }

}
