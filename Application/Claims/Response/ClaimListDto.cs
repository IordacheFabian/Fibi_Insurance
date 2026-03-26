using System;
using Domain.Models.Claims;

namespace Application.Claims.Response;

public class ClaimListDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public ClaimStatus Status { get; set; }
    public decimal EstimatedDamage { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public DateOnly CreatedAt { get; set; }
}
