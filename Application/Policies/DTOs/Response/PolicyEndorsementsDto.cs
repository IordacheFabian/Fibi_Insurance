using System;

namespace Application.Policies.DTOs.Response;

public class PolicyEndorsementsDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public int VersionNumber { get; set; }
    public DateOnly EffectiveDate { get; set; }
    // public decimal PremiumDifference { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
