using System;

namespace Application.Claims.Response;

public class PaidClaimDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public decimal EstimatedDamage { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public DateOnly? ApprovedAt { get; set; }
    public DateOnly? PaidAt { get; set; }
}