using System;

namespace Application.Claims.Response;

public class PaidClaimDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public decimal EstimatedDamage { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}