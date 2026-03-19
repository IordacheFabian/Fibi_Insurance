using System;
using Domain.Models.Claims;

namespace Application.Claims.Response;

public class RejectedClaimDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public ClaimStatus Status { get; set; }
    public decimal EstimatedDamage { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
