using System;
using Application.Core.Interfaces.IRepositories;
using Domain.Models.Buildings;
using Domain.Models.Metadatas;
using Domain.Models.Policies;

namespace Persistence.Repositories;

public class PremiumCalculator(IMetadataRepository metadataRepository) : IPremiumCalculator
{
    public async Task<(decimal finalPremium, List<PolicyAdjustement> policyAdjustements)> CalculateAsync(Building building, decimal basePremium, DateOnly startDate, CancellationToken cancellationToken)
    {
        var policyAdjustements = new List<PolicyAdjustement>();

        var fees = await metadataRepository.GetActiveFeeConfigurationsAsync(startDate, cancellationToken);

        foreach (var fee in fees)
        {
            var amount = Math.Round(basePremium * fee.Percentage / 100m, 2);
            policyAdjustements.Add(new PolicyAdjustement
            {
                Id = Guid.NewGuid(),
                Name = fee.Name,
                AdjustementType = AdjustementType.AdminFee,
                Percentage = fee.Percentage,
                Amount = amount,
            });
        }

        var riskFactors = await metadataRepository.GetActiveRiskFactorConfigurationsAsync(cancellationToken);

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

            var amount = Math.Round(basePremium * riskFactor.AdjustementPercentage / 100m, 2);
            policyAdjustements.Add(new PolicyAdjustement
            {
                Id = Guid.NewGuid(),
                Name = $"Risk factor - {riskFactor.RiskLevel}",
                AdjustementType = AdjustementType.RiskAdjustement,
                Percentage = riskFactor.AdjustementPercentage,
                Amount = amount,
            });
        }

        var totalPolicyAdjustements = policyAdjustements.Sum(x => x.Amount);
        var finalPremium = Math.Round(basePremium + totalPolicyAdjustements, 2);

        return (finalPremium, policyAdjustements);
    }
}
