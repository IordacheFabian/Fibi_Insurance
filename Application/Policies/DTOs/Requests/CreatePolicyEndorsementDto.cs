using System;
using Domain.Models.Policies;

namespace Application.Policies.DTOs.Requests;

public class CreatePolicyEndorsementDto
{
    public EndorsementType EndorsementType { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string Reason { get; set; } = default!;

    public decimal? NewBasePremium { get; set; }
    public DateOnly? NewStartDate { get; set; }
    public DateOnly? NewEndDate { get; set; }

    public decimal? ManualAdjustementPercentage { get; set; }

}
