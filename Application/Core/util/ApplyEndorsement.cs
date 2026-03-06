using System;
using Application.Policies.DTOs.Requests;
using Domain.Models.Policies;

namespace Application.Core.util;

public class ApplyEndorsement
{
    public static void ApplyEndorsementChanges(CreatePolicyEndorsementDto createPolicyEndorsementDto, PolicyVersion newPolicyVersion)
    {
        switch (createPolicyEndorsementDto.EndorsementType)
        {
            case EndorsementType.InsuredValueChange:
                if (!createPolicyEndorsementDto.NewBasePremium.HasValue || createPolicyEndorsementDto.NewBasePremium.Value <= 0)
                {
                    throw new BadRequestException("NewBasePremium must be provided and > 0");
                }
                newPolicyVersion.BasePremium = createPolicyEndorsementDto.NewBasePremium.Value;
                break;

            case EndorsementType.PeriosExtension:
                if (!createPolicyEndorsementDto.NewStartDate.HasValue || !createPolicyEndorsementDto.NewEndDate.HasValue)
                {
                    throw new BadRequestException("NewStartDate and NewEndDate must be provided");
                }
                newPolicyVersion.StartDate = createPolicyEndorsementDto.NewStartDate.Value;
                newPolicyVersion.EndDate = createPolicyEndorsementDto.NewEndDate.Value;
                break;

            case EndorsementType.ManualAdjustement:
                if (!createPolicyEndorsementDto.ManualAdjustementPercentage.HasValue)
                {
                    throw new BadRequestException("ManualAdjustementPercentage must be provided");
                }
                newPolicyVersion.PolicyAdjustments.Add(new PolicyAdjustment
                {
                    Id = Guid.NewGuid(),
                    Name = "Manual Adjustement",
                    AdjustmentType = AdjustmentType.AdminFee,
                    Percentage = createPolicyEndorsementDto.ManualAdjustementPercentage.Value,
                    Amount = 0m
                });
                break;

            default:
                throw new BadRequestException("Invalid endorsement type");

        }
    }
}
