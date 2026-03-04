using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Buildings;
using Domain.Models.Metadatas;
using Domain.Models.Policies;

namespace Persistence.Repositories;

public class PremiumCalculator(
    IRiskFactorRepository riskFactorRepository,
    IFeeConfigurationRepository feeConfigurationRepository) : IPremiumCalculator
{
    public async Task<(decimal finalPremium, List<PolicyAdjustment> policyAdjustments)> CalculateAsync(Building building, decimal basePremium, DateOnly startDate, CancellationToken cancellationToken)
    {
        var policyAdjustments = new List<PolicyAdjustment>();

        var fees = await feeConfigurationRepository.GetActiveFeeConfigurationsAsync(startDate, cancellationToken);

        foreach (var fee in fees)
        {
            var amount = RoundAmount(basePremium * fee.Percentage / 100m);
            policyAdjustments.Add(new PolicyAdjustment
            {
                Id = Guid.NewGuid(),
                Name = fee.Name,
                AdjustmentType = AdjustmentType.AdminFee,
                Percentage = fee.Percentage,
                Amount = amount,
            });
        }

        var riskFactors = await riskFactorRepository.GetActiveRiskFactorConfigurationsAsync(cancellationToken);

        foreach (var riskFactor in riskFactors)
        {
            bool matches = false;

            if(riskFactor.RiskLevel == RiskLevel.BuildingType && riskFactor.BuildingType.HasValue)
            {
                matches = riskFactor.BuildingType.Value == building.BuildingType;
            }
            else if (riskFactor.RiskLevel == RiskLevel.City && riskFactor.ReferenceID == building.Address.CityId)
            {
                matches = true;
            }
            else if (riskFactor.RiskLevel == RiskLevel.County && riskFactor.ReferenceID == building.Address.City.CountyId)
            {
                matches = true;
            }
            else if (riskFactor.RiskLevel == RiskLevel.Country && riskFactor.ReferenceID == building.Address.City.County.CountryId)
            {
                matches = true;
            }

            if (!matches) continue;

            var amount = RoundAmount(basePremium * riskFactor.AdjustementPercentage / 100m);
            policyAdjustments.Add(new PolicyAdjustment
            {
                Id = Guid.NewGuid(),
                Name = $"Risk factor - {riskFactor.RiskLevel}",
                AdjustmentType = AdjustmentType.RiskAdjustment,
                Percentage = riskFactor.AdjustementPercentage,
                Amount = amount,
            });
        }

        var totalPolicyAdjustments = policyAdjustments.Sum(x => x.Amount);
        var finalPremium = RoundAmount(Math.Max(0, basePremium + totalPolicyAdjustments));

        return (finalPremium, policyAdjustments);
    }

    private static decimal RoundAmount(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
