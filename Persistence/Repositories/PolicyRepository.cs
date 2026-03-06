using System;
using Application.Addresses.DTOs;
using Application.Brokers.DTOs.Response;
using Application.Buildings.DTOs.Response;
using Application.Clients.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Policies.DTOs.Requests;
using Application.Policies.DTOs.Response;
using Domain.Models.Metadatas;
using Domain.Models.Policies;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class PolicyRepository(AppDbContext context) : IPolicyRepository
{
    public async Task CreatePolicyAsync(Policy policy, CancellationToken cancellationToken)
    {
        await context.Policies.AddAsync(policy, cancellationToken);
    }

    public async Task CreatePolicyEndorsementAsync(PolicyEndorsement policyEndorsement, CancellationToken cancellationToken)
    {
        await context.PolicyEndorsements.AddAsync(policyEndorsement, cancellationToken);
    }

    public async Task<Policy?> GetPolicyAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
 
    public async Task<PolicyDetailsDto?> GetPolicyDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                Policy = p,
                ActiveVersion = p.PolicyVersions
                    .Where(v => v.IsActiveVersion)
                    .Select(v => new
                    {
                        v.VersionNumber,
                        v.StartDate,
                        v.EndDate,
                        v.BasePremium,
                        v.FinalPremium,
                        CurrencyCode = v.Currency.Code,
                        CurrencyName = v.Currency.Name,
                        PolicyAdjustments = v.PolicyAdjustments
                            .Select(a => new PolicyAdjustmentDto
                            {
                                Name = a.Name,
                                AdjustmentType = a.AdjustmentType,
                                Percentage = a.Percentage,
                                Amount = a.Amount
                            })
                            .ToList()
                    })
                    .SingleOrDefault()
            })
            .Where(x => x.ActiveVersion != null)
        .Select(x => new PolicyDetailsDto
        {
            Id = x.Policy.Id,
            PolicyNumber = x.Policy.PolicyNumber,
            PolicyStatus = x.Policy.PolicyStatus,

            StartDate = x.ActiveVersion!.StartDate,
            EndDate = x.ActiveVersion!.EndDate,
            BasePremium = x.ActiveVersion!.BasePremium,
            FinalPremium = x.ActiveVersion!.FinalPremium,
            CurrencyName = x.ActiveVersion!.CurrencyName,
            CurrencyCode = x.ActiveVersion!.CurrencyCode,

            VersionNumber = x.ActiveVersion!.VersionNumber,

            // CancelledAt = x.Policy.PolicyVersion.,
            // CancellationReason = x.Policy.CancellationReason,

            Client = new ClientDetailsDto
            {
                Id = x.Policy.Client.Id,
                Name = x.Policy.Client.Name,
                Email = x.Policy.Client.Email,
                PhoneNumber = x.Policy.Client.PhoneNumber,
                IdentificationNumber = x.Policy.Client.IdentificationNumber,
            },

            Building = new BuildingDetailsDto
            {
                Id = x.Policy.Building.Id,
                BuildingType = x.Policy.Building.BuildingType,
                InsuredValue = x.Policy.Building.InsuredValue,
                Address = new AddressDetailsDto
                {
                    Street = x.Policy.Building.Address.Street,
                    CityName = x.Policy.Building.Address.City.Name,
                    Number = x.Policy.Building.Address.Number
                },
                Owner = new ClientDetailsDto
                {
                    Id = x.Policy.Building.Client.Id,
                    Type = x.Policy.Building.Client.ClientType,
                    Name = x.Policy.Building.Client.Name,
                    IdentificationNumber = x.Policy.Building.Client.IdentificationNumber,
                    Email = x.Policy.Building.Client.Email,
                    PhoneNumber = x.Policy.Building.Client.PhoneNumber,
                },
                CounstructionYear = x.Policy.Building.ConstructionYear,
                NumberOfFloors = x.Policy.Building.NumberOfFloors,
                SurfaceArea = x.Policy.Building.SurfaceArea,
                RiskIndicatiors = x.Policy.Building.RiskIndicatiors
            },

            Broker = new BrokerDto
            {
                Id = x.Policy.Broker.Id,
                Name = x.Policy.Broker.Name,
                BrokerCode = x.Policy.Broker.BrokerCode
            },

            PolicyAdjustments = x.ActiveVersion!.PolicyAdjustments
        })
        .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Policy?> GetPolicyForEndorsementAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
            .AsTracking()
            .Include(p => p.Building)
                .ThenInclude(b => b.Address)
                    .ThenInclude(a => a.City)
                        .ThenInclude(c => c.County)
                            .ThenInclude(co => co.Country)
            .Include(p => p.PolicyVersions.Where(v => v.IsActiveVersion))
                .ThenInclude(v => v.Currency)
            .Include(p => p.PolicyVersions.Where(v => v.IsActiveVersion))
                .ThenInclude(v => v.PolicyAdjustments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Policy?> GetPolicyForActivationAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
            .Include(p => p.PolicyVersions.Where(v => v.IsActiveVersion))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Policy?> GetPolicyForCancellationAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Policies
            .Include(p => p.PolicyVersions.Where(v => v.IsActiveVersion))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public IQueryable<Policy> ListPolicyAsync(Guid? clientId, Guid? brokerId, PolicyStatus? policyStatus, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken)
    {
        var query = context.Policies   
            .Include(x => x.PolicyVersions.Where(x => x.IsActiveVersion))
            .AsNoTracking()
            .AsQueryable();

            if (clientId.HasValue) 
                query = query.Where(x => x.ClientId == clientId.Value);
            
            if(brokerId.HasValue)
                query = query.Where(x => x.BrokerId == brokerId.Value);

            if(policyStatus.HasValue)
                query = query.Where(x => x.PolicyStatus == policyStatus.Value);

            if(startDate.HasValue) 
                query = query.Where(x => x.PolicyVersions.Any(pv => pv.StartDate >= startDate.Value));

            if(endDate.HasValue)
                query = query.Where(x => x.PolicyVersions.Any(pv => pv.EndDate <= endDate.Value));

            query = query
                .Include(x => x.Client)
                .Include(x => x.Building)
                    .ThenInclude(x => x.Address)
                        .ThenInclude(x => x.City)
                .Include(x => x.PolicyVersions)
                    .ThenInclude(x => x.Currency);

            return query; 
            
        }

    public async Task CreatePolicyVersionAsync(PolicyVersion policyVersion, CancellationToken cancellationToken)
    {
        await context.PolicyVersions.AddAsync(policyVersion, cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

}
