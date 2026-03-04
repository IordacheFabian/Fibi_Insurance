using System;
using Domain.Models.Metadatas;

namespace Domain.Models.Policies;

public class PolicyVersion
{
    public Guid Id { get; set; } 
    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = default!;

    public int VersionNumber {get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public decimal BasePremium { get; set; }
    public decimal FinalPremium { get; set; }

    public Guid CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = default!;

    public bool IsActiveVersion { get; set; }

    public ICollection<PolicyAdjustment> PolicyAdjustments { get; set; } = new HashSet<PolicyAdjustment>();

}
