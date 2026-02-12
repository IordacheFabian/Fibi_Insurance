using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Policies.DTOs.Requests;
using AutoMapper;
using Domain.Models.Policies;
using MediatR;

namespace Application.Policies.DTOs.Command;

public class CreatePolicyDraft
{
    public class Command : IRequest<Guid>
    {
        public required CreatePolicyDraftDto CreatePolicyDraftDto { get; set; }
    }

    public class Handler(
        IPolicyRepository policyRepository,
        IClientRepository clientRepository,
        IBuildingRepository buildingRepository,
        ICurrencyRepository currencyRepository,
        IPremiumCalculator premiumCalculator) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            var policyDto = request.CreatePolicyDraftDto;

            var client = await clientRepository.GetClientAsync(policyDto.ClientId, cancellationToken);
            if (client == null) throw new NotFoundException("Client not found");

            var building = await buildingRepository.GetBuildingAsync(policyDto.BuildingId, cancellationToken);
            if (building == null) throw new NotFoundException("Building not found");

            var currency = await currencyRepository.GetCurrencyAsync(policyDto.CurrencyId, cancellationToken);
            if (currency == null) throw new NotFoundException("Currency not found");

            if(policyDto.EndDate <= policyDto.StartDate) throw new BadRequestException("End date must be after start date");

            if(policyDto.BasePremium <= 0) throw new BadRequestException("Base premium must be greater than zero");

            var (finalPremium, policyAdjustements) = await premiumCalculator
                    .CalculateAsync(building, policyDto.BasePremium, policyDto.StartDate, cancellationToken);

            var policyNumber = GeneratePolicyNumber();
            policyDto.PolicyNumber = policyNumber;
            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = policyNumber,
                ClientId = policyDto.ClientId,
                BuildingId = policyDto.BuildingId,
                CurrencyId = policyDto.CurrencyId,
                BrokerId = policyDto.BrokerId,

                PolicyStatus = PolicyStatus.Draft,
                StartDate = policyDto.StartDate,
                EndDate = policyDto.EndDate,

                BasePremium = policyDto.BasePremium,
                FinalPremium = finalPremium,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var policyAdjustement in policyAdjustements)
            {
                policyAdjustement.Policy = policy;
                policy.PolicyAdjustements.Add(policyAdjustement);
            }

            await policyRepository.CreatePolicyAsync(policy, cancellationToken);  

            var result = await policyRepository.SaveChangesAsync(cancellationToken);
            if (!result) throw new Exception("Failed to create policy draft");

            return policy.Id; 
        }

        public static string GeneratePolicyNumber()
        {
            return $"POL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".ToUpper();
        }
    }

}
