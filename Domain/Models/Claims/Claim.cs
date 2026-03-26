using System;
using Domain.Models.Policies;

namespace Domain.Models.Claims;

public class Claim
{
    public Guid Id { get; set; } 
    
    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = default!;

    public string Description { get; set; } = default!; 
    public DateOnly IncidentDate { get; set; }
    
    public decimal EstimatedDamage { get; set; }
    public decimal? ApprovedAmount { get; set; }

    public ClaimStatus Status { get; set; }

    public string? RejectionReason { get; set; }

    public DateOnly CreatedAt { get; set; }
    public DateOnly? ReviewedAt { get; set; }
    public DateOnly? ApprovedAt { get; set; }
    public DateOnly? RejectedAt { get; set; }
    public DateOnly? PaidAt { get; set; }
}
