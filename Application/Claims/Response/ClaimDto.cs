using System;

namespace Application.Claims.Response;

public class ClaimDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string Description { get; set; } = default!;
    public DateOnly IncidentDate { get; set; }
    public decimal EstimatedDamage { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string Status { get; set; } = default!;
    public string? RejectionReason { get; set; }
    public DateOnly CreatedAt { get; set; }
    public DateOnly? ReviewedAt { get; set; }
    public DateOnly? ApprovedAt { get; set; }
    public DateOnly? RejectedAt { get; set; }
    public DateOnly? PaidAt { get; set; }
}
