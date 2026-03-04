using System;
using Domain.Models.Buildings;
using Domain.Models.Policies;

namespace Application.Core.Interfaces.IRepositories;

public interface IPremiumCalculator
{
    Task<(decimal finalPremium, List<PolicyAdjustment> policyAdjustments)> CalculateAsync(
        Building building,
        decimal basePremium,
        DateOnly startDate,
        CancellationToken cancellationToken
    );
}
