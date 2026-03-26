using System;

namespace Application.Claims.Request;

public class CreateClaimDto
{
    public string Description { get; set; } = default!;
    public DateOnly IncidentDate { get; set; }
    public decimal EstimatedDamage { get; set; }
}
