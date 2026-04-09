using System;
using Application.Brokers.DTOs.Response;
using Application.Buildings.DTOs.Response;
using Application.Clients.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Metadatas.Currencies.DTOs.Response;
using Application.Policies.DTOs.Requests;
using Application.Policies.DTOs.Response;
using AutoMapper;
using Domain.Models.Brokers;
using Domain.Models.Policies;
using MediatR;

namespace Application.Policies.Command;

public class CreatePolicyDraft
{
    public class Command : IRequest<PolicyDetailsDto>
    {
        public Guid BrokerId { get; set; }
        public required CreatePolicyDraftDto CreatePolicyDraftDto { get; set; }
    }

    public class Handler(
        IPolicyRepository policyRepository,
        IClientRepository clientRepository,
        IBuildingRepository buildingRepository,
        ICurrencyRepository currencyRepository,
        IBrokerRepository brokerRepository,
        IPremiumCalculator premiumCalculator,
        IMapper mapper) : IRequestHandler<Command, PolicyDetailsDto>
    {
        public async Task<PolicyDetailsDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var policyDto = request.CreatePolicyDraftDto;

            var client = await clientRepository.GetClientAsync(policyDto.ClientId, request.BrokerId, cancellationToken);
            if (client == null) throw new NotFoundException("Client not found");

            var building = await buildingRepository.GetBuildingAsync(policyDto.BuildingId, request.BrokerId, cancellationToken);
            if (building == null) throw new NotFoundException("Building not found");

            var currency = await currencyRepository.GetCurrencyAsync(policyDto.CurrencyId, cancellationToken);
            if (currency == null) throw new NotFoundException("Currency not found");

            var broker = await brokerRepository.GetBrokerAsync(request.BrokerId, cancellationToken);
            if (broker == null) throw new NotFoundException("Broker not found");
            if (broker.BrokerStatus != BrokerStatus.Active)
                throw new BadRequestException("Broker is not active");

            if(policyDto.EndDate <= policyDto.StartDate) throw new BadRequestException("End date must be after start date");

            if(policyDto.BasePremium <= 0) throw new BadRequestException("Base premium must be greater than zero");

            var (finalPremium, policyAdjustements) = await premiumCalculator
                    .CalculateAsync(building, policyDto.BasePremium, policyDto.StartDate, cancellationToken);

            var clientDto = mapper.Map<ClientDetailsDto>(client);
            var buildingDto = mapper.Map<BuildingDetailsDto>(building);
            var currencyDto = mapper.Map<CurrencyDto>(currency);
            var brokerDto = mapper.Map<BrokerDetailsDto>(broker);

            var policyNumber = GeneratePolicyNumber();
            policyDto.PolicyNumber = policyNumber;
            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = policyNumber,
                ClientId = policyDto.ClientId,
                BuildingId = policyDto.BuildingId,
                BrokerId = request.BrokerId,
                PolicyVersions = new List<PolicyVersion>
                {
                    new PolicyVersion
                    {
                        Id = Guid.NewGuid(),
                        VersionNumber = 1,
                        StartDate = policyDto.StartDate,
                        EndDate = policyDto.EndDate,
                        BasePremium = policyDto.BasePremium,
                        FinalPremium = finalPremium,
                        CurrencyId = policyDto.CurrencyId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = broker.Name,
                        IsActiveVersion = true
                    }
                },
                PolicyStatus = PolicyStatus.Draft,
            };

            var version = policy.PolicyVersions.First();
            foreach (var policyAdjustement in policyAdjustements)
            {
                policyAdjustement.PolicyVersion = version;
                version.PolicyAdjustments.Add(policyAdjustement);
            }

            await policyRepository.CreatePolicyAsync(policy, cancellationToken);  

            var result = await policyRepository.SaveChangesAsync(cancellationToken);
            if (!result) throw new Exception("Failed to create policy draft");

            var finalPolicy = mapper.Map<PolicyDetailsDto>(policy); 
            finalPolicy.Client = clientDto;
            finalPolicy.Building = buildingDto;
            finalPolicy.CurrencyCode = currencyDto.Code;
            finalPolicy.CurrencyName = currencyDto.Name;

            return finalPolicy;
        }

        public static string GeneratePolicyNumber()
        {
            return $"POL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".ToUpper();
        }
    }

}
