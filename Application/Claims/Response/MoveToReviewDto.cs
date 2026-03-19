using System;
using Domain.Models.Claims;

namespace Application.Claims.Response;

public class MoveToReviewDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public ClaimStatus Status { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
