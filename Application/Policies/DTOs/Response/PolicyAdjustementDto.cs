using System;
using Domain.Models.Policies;

namespace Application.Policies.DTOs.Response;

public class PolicyAdjustementDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public AdjustementType AdjustementType { get; set; }
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
}
