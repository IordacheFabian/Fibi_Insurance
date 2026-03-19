using System;
using Domain.Models.Policies;

namespace Domain.Models.Claims;

public class Claim
{
    public Guid Id { get; set; } 
    
    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = default!;

    public string Description { get; set; } = default!; 
    public DateTime IncidentDate { get; set; }
    
    public decimal EstimatedDamage { get; set; }
    public decimal? ApprovedAmount { get; set; }

    public ClaimStatus Status { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
