using System;
using Domain.Models.Policies;

namespace Application.Policies.DTOs.Response;

public class PolicyEndorsementsDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public EndorsementType EndorsementType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int OldVersionNumber { get; set; }
    public int VersionNumber { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public decimal PreviousFinalPremium { get; set; }
    public decimal NewFinalPremium { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public PolicyStatus PolicyStatus { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
