using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Claims;
using Application.Addresses;
using Application.Addresses.DTOs;
using Application.Brokers.DTOs.Request;
using Application.Brokers.DTOs.Response;
using Application.Buildings.DTOs.Request;
using Application.Buildings.DTOs.Response;
using Application.Claims.Response;
using Application.Clients.DTOs;
using Application.Clients.DTOs.Response;
using Application.Core.util;
using Application.Geographies.DTOs;
using Application.Metadatas.Currencies.DTOs.Request;
using Application.Metadatas.Currencies.DTOs.Response;
using Application.Metadatas.Fees.DTOs.Request;
using Application.Metadatas.Fees.DTOs.Response;
using Application.Metadatas.RiskFactors.DTOs.Request;
using Application.Metadatas.RiskFactors.DTOs.Response;
using Application.Payments.Response;
using Application.Policies.DTOs.Requests;
using Application.Policies.DTOs.Response;
using AutoMapper;
using Domain.Models;
using Domain.Models.Brokers;
using Domain.Models.Buildings;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;
using Domain.Models.Metadatas;
using Domain.Models.Payments;
using Domain.Models.Policies;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {

        //client mappings
        CreateMap<CreateClientDto, Client>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Addresses, opt => opt.Ignore())
            .ForMember(d => d.Buildings, opt => opt.Ignore());
        CreateMap<UpdateClientDto, Client>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ClientType, opt => opt.Ignore())
            .ForMember(d => d.IdentificationNumber, opt => opt.Ignore())
            .ForMember(d => d.Addresses, opt => opt.Ignore())
            .ForMember(d => d.Buildings, opt => opt.Ignore());

        CreateMap<Client, ClientDetailsDto>()
            .ForMember(d => d.Type, opt => opt.MapFrom(src => src.ClientType));

        CreateMap<Client, ClientSearchDto>() 
            .ForMember(d => d.IdentificationMasked,
                opt => opt.MapFrom(src => IdentificationMasker.Mask(src.IdentificationNumber, 4)));

        //building mappings
        CreateMap<CreateBuildingDto, Building>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.AddressId, opt => opt.Ignore())
            .ForMember(d => d.Address, opt => opt.Ignore())
            .ForMember(d => d.Client, opt => opt.Ignore())
            .ForMember(d => d.RiskIndicatiors, opt => opt.MapFrom(src => src.RiskIndicators));
        CreateMap<UpdateBuildingDto, Building>()
            .ForMember(d => d.ClientId, opt => opt.Ignore())
            .ForMember(d => d.AddressId, opt => opt.Ignore())
            .ForMember(d => d.Client, opt => opt.Ignore())
            .ForMember(d => d.Address, opt => opt.Ignore());

        CreateMap<Building, BuildingDetailsDto>()
            .ForMember(d => d.Owner, 
                opt => opt.MapFrom(src => src.Client))
            .ForMember(d => d.ConstructionYear,
                opt => opt.MapFrom(src => src.ConstructionYear))
            .ForMember(d => d.RiskIndicators,
                opt => opt.MapFrom(src => src.RiskIndicatiors));
        CreateMap<Building, BuildingListDto>()
            .ForMember(d => d.Address,
                opt => opt.MapFrom(src => $"{src.Address.Street} {src.Address.Number}"))
            .ForMember(d => d.CityName,
                opt => opt.MapFrom(src => src.Address.City.Name));
        
        CreateMap<Building, ClientBuildingSummaryDto>()
            .ForMember(d => d.Address, 
                opt => opt.MapFrom(src => $"{src.Address.Street} {src.Address.Number}"))
            .ForMember(d => d.CityName,
                opt => opt.MapFrom(src => src.Address.City.Name));
      
        //address mappingss
        CreateMap<CreateAddressDto, Address>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Building, opt => opt.Ignore())
            .ForMember(d => d.City, opt => opt.Ignore())
            .ForMember(d => d.IsPrimary, opt => opt.Ignore());
        CreateMap<UpdateAddressDto, Address>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Building, opt => opt.Ignore())
            .ForMember(d => d.City, opt => opt.Ignore())
            .ForMember(d => d.IsPrimary, opt => opt.Ignore());

        CreateMap<Address, AddressDetailsDto>()
            .ForMember(d => d.CityName,
             opt => opt.MapFrom(src => src.City.Name));

        //geo mappings
        CreateMap<Building, GeographyDto>()
            .ForMember(d => d.CityId,
                opt => opt.MapFrom(src => src.Address.City.Id))
            .ForMember(d => d.CityName,
                opt => opt.MapFrom(src => src.Address.City.Name))
            .ForMember(d => d.CountyId, 
                opt => opt.MapFrom(src => src.Address.City.County.Id))
            .ForMember(d => d.CountyName,
                opt => opt.MapFrom(src => src.Address.City.County.Name))
            .ForMember(d => d.CountryId,
                opt => opt.MapFrom(src => src.Address.City.County.Country.Id))
            .ForMember(d => d.CountryName, 
                opt => opt.MapFrom(src => src.Address.City.County.Country.Name));

        CreateMap<Country, CountryDto>();
        CreateMap<County, CountyDto>();
        CreateMap<City, CityDto>();

        // policies mappings
        CreateMap<Policy, PolicyListItemDto>()
            .ForMember(d => d.ClientName, o => o.MapFrom(s => s.Client.Name))
            .ForMember(d => d.BuildingStreet, o => o.MapFrom(s => s.Building.Address.Street))
            .ForMember(d => d.BuildingNumber, o => o.MapFrom(s => s.Building.Address.Number))
            .ForMember(d => d.CityName, o => o.MapFrom(s => s.Building.Address.City.Name))
            .ForMember(d => d.CurrencyId, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).CurrencyId))
            .ForMember(d => d.CurrencyCode, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).Currency.Code))
            .ForMember(d => d.StartDate, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).StartDate))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).EndDate))
            .ForMember(d => d.BasePremium, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).BasePremium))
            .ForMember(d => d.FinalPremium, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).FinalPremium));

        CreateMap<Policy, PolicyDetailsDto>()
            .ForMember(d => d.Client, o => o.MapFrom(s => s.Client))
            .ForMember(d => d.Building, o => o.MapFrom(s => s.Building))
            .ForMember(d => d.Broker, o => o.MapFrom(s => s.Broker))
            .ForMember(d => d.VersionNumber, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).VersionNumber))
            .ForMember(d => d.StartDate, o => o.MapFrom(s =>
                s.PolicyVersions.Single(v => v.IsActiveVersion).StartDate))
            .ForMember(d => d.EndDate, o => o.MapFrom(s =>
                s.PolicyVersions.Single(v => v.IsActiveVersion).EndDate))
            .ForMember(d => d.BasePremium, o => o.MapFrom(s =>
                s.PolicyVersions.Single(v => v.IsActiveVersion).BasePremium))
            .ForMember(d => d.FinalPremium, o => o.MapFrom(s =>
                s.PolicyVersions.Single(v => v.IsActiveVersion).FinalPremium))
            .ForMember(d => d.CurrencyCode, o => o.MapFrom(s =>
                s.PolicyVersions.Single(v => v.IsActiveVersion).Currency.Code))
            .ForMember(d => d.CurrencyName, o => o.MapFrom(s =>
                s.PolicyVersions.Single(v => v.IsActiveVersion).Currency.Name))
            .ForMember(d => d.PolicyAdjustments, o => o.MapFrom(s =>
        s.PolicyVersions.Single(v => v.IsActiveVersion).PolicyAdjustments))
            .ForMember(d => d.CancelledAt, o => o.Ignore())
            .ForMember(d => d.CancellationReason, o => o.Ignore());

        CreateMap<PolicyAdjustment, PolicyAdjustmentDto>();
        
        CreateMap<Policy, CreatePolicyDraftDto>()
            .ForMember(d => d.ClientId, o => o.MapFrom(s => s.Client.Id))
            .ForMember(d => d.BuildingId, o => o.MapFrom(s => s.Building.Id))
            .ForMember(d => d.CurrencyId, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).Currency.Id))
            .ForMember(d => d.BrokerId, o => o.MapFrom(s => s.Broker.Id))
            .ForMember(d => d.BasePremium, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).BasePremium))
            .ForMember(d => d.StartDate, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).StartDate))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => s.PolicyVersions.Single(v => v.IsActiveVersion).EndDate));
        
        CreateMap<Policy, ActivatePolicyDto>()
            .ForMember(d => d.ActivationDate, opt => opt.Ignore());
        CreateMap<Policy, CancelPolicyDto>()
            .ForMember(d => d.CancellationDate, opt => opt.Ignore())
            .ForMember(d => d.CancellationReason, opt => opt.Ignore());
        CreateMap<PolicyEndorsement, CreatePolicyEndorsementDto>()
            .ForMember(d => d.NewBasePremium, opt => opt.Ignore())
            .ForMember(d => d.NewStartDate, opt => opt.Ignore())
            .ForMember(d => d.NewEndDate, opt => opt.Ignore())
            .ForMember(d => d.ManualAdjustementPercentage, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<PolicyEndorsement, PolicyEndorsementsDto>()
            .ForMember(d => d.VersionNumber, o => o.MapFrom(s => s.NewVersionNumber));
        CreateMap<PolicyVersion, PolicyVersionsDto>()
            .ForMember(d => d.CurrencyCode, o => o.MapFrom(s => s.Currency.Code));

        // metadata and premium calculator mappings
        CreateMap<Currency, CurrencyDto>();
        CreateMap<Currency, CreateCurrencyDto>();
        CreateMap<CreateCurrencyDto, Currency>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Policies, opt => opt.Ignore());
        CreateMap<Currency, UpdateCurrencyDto>();
        CreateMap<UpdateCurrencyDto, Currency>()
            .ForMember(d => d.Code, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Policies, opt => opt.Ignore());   

        CreateMap<FeeConfiguration, FeeConfigurationDto>();
        CreateMap<FeeConfiguration, CreateFeeConfigurationDto>();
        CreateMap<CreateFeeConfigurationDto, FeeConfiguration>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());   
        CreateMap<FeeConfiguration, UpdateFeeConfigurationDto>();
        CreateMap<UpdateFeeConfigurationDto, FeeConfiguration>()
            .ForMember(d => d.FeeType, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

        CreateMap<RiskFactorConfiguration, RiskFactorDto>();
        CreateMap<RiskFactorConfiguration, CreateRiskFactorDto>()
            .ForMember(d => d.Level, opt => opt.MapFrom(src => src.RiskLevel))
            .ForMember(d => d.ReferenceId, opt => opt.MapFrom(src => src.ReferenceID));
        CreateMap<CreateRiskFactorDto, RiskFactorConfiguration>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.RiskLevel, opt => opt.MapFrom(src => src.Level))
            .ForMember(d => d.ReferenceID, opt => opt.MapFrom(src => src.ReferenceId));
        CreateMap<RiskFactorConfiguration, UpdateRiskFactorDto>();
        CreateMap<UpdateRiskFactorDto, RiskFactorConfiguration>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ReferenceID, opt => opt.Ignore())
            .ForMember(d => d.BuildingType, opt => opt.Ignore());

        // broker mappings
        CreateMap<Broker, BrokerDto>();
        CreateMap<Broker, BrokerDetailsDto>();
        CreateMap<CreateBrokerDto, Broker>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Policies, opt => opt.Ignore());
        CreateMap<Broker, CreateBrokerDto>();
        CreateMap<UpdateBrokerDto, Broker>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.BrokerCode, opt => opt.Ignore())
            .ForMember(d => d.BrokerStatus, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Policies, opt => opt.Ignore());
        CreateMap<Broker, UpdateBrokerDto>();

        // payment mappings
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Method, opt => opt.MapFrom(src => src.Method.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.ToString()));

    }
}
