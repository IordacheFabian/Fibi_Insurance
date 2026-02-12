using System;
using System.Runtime.InteropServices;
using Application.Addresses;
using Application.Addresses.DTOs;
using Application.Brokers.DTOs.Request;
using Application.Brokers.DTOs.Response;
using Application.Buildings.DTOs.Request;
using Application.Buildings.DTOs.Response;
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
using Application.Policies.DTOs.Requests;
using Application.Policies.DTOs.Response;
using AutoMapper;
using Domain.Models;
using Domain.Models.Brokers;
using Domain.Models.Buildings;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;
using Domain.Models.Metadatas;
using Domain.Models.Policies;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {

        //client mappings
        CreateMap<CreateClientDto, Client>();
        CreateMap<UpdateClientDto, Client>();

        CreateMap<Client, ClientDetailsDto>();

        CreateMap<Client, ClientSearchDto>() 
            .ForMember(d => d.IdentificationMasked,
                opt => opt.MapFrom(src => IdentificationMasker.Mask(src.IdentificationNumber, 4)));

        //building mappings
        CreateMap<CreateBuildingDto, Building>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Address, opt => opt.Ignore());
        CreateMap<UpdateBuildingDto, Building>();

        CreateMap<Building, BuildingDetailsDto>()
            .ForMember(d => d.Owner, 
                opt => opt.MapFrom(src => src.Client));
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
            .ForMember(d => d.Id, opt => opt.Ignore());
        CreateMap<UpdateAddressDto, Address>();

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
            .ForMember(d => d.ClientName,
                o => o.MapFrom(src => src.Client.Name))
            .ForMember(d => d.BuildingStreet, o => o.MapFrom(s => s.Building.Address.Street))
            .ForMember(d => d.BuildingNumber, o => o.MapFrom(s => s.Building.Address.Number))
            .ForMember(d => d.CityName, o => o.MapFrom(s => s.Building.Address.City.Name))
            .ForMember(d => d.CurrencyCode, o => o.MapFrom(s => s.Currency.Code));

        CreateMap<Policy, PolicyDetailsDto>()
                .ForMember(d => d.CurrencyCode, o => o.MapFrom(s => s.Currency.Code))
                .ForMember(d => d.CurrencyName, o => o.MapFrom(s => s.Currency.Name))
                .ForMember(d => d.Client, o => o.MapFrom(s => s.Client))
                .ForMember(d => d.Building, o => o.MapFrom(s => s.Building))
                .ForMember(d => d.Broker, o => o.MapFrom(s => s.Broker))
                .ForMember(d => d.PolicyAdjustements, o => o.MapFrom(s => s.PolicyAdjustements));

        CreateMap<PolicyAdjustement, PolicyAdjustementDto>();
        
        CreateMap<Policy, CreatePolicyDraftDto>()
            .ForMember(d => d.ClientId, o => o.MapFrom(s => s.Client.Id))
            .ForMember(d => d.BuildingId, o => o.MapFrom(s => s.Building.Id))
            .ForMember(d => d.CurrencyId, o => o.MapFrom(s => s.Currency.Id))
            .ForMember(d => d.BrokerId, o => o.MapFrom(s => s.Broker.Id));
        
        CreateMap<Policy, ActivatePolicyDto>();
        CreateMap<Policy, CancelPolicyDto>();

        // metadata and premium calculator mappings
        CreateMap<Currency, CurrencyDto>();
        CreateMap<Currency, CreateCurrencyDto>();
        CreateMap<CreateCurrencyDto, Currency>();
        CreateMap<Currency, UpdateCurrencyDto>();
        CreateMap<UpdateCurrencyDto, Currency>();   

        CreateMap<FeeConfiguration, FeeConfigurationDto>();
        CreateMap<FeeConfiguration, CreateFeeConfigurationDto>();
        CreateMap<CreateFeeConfigurationDto, FeeConfiguration>();   
        CreateMap<FeeConfiguration, UpdateFeeConfigurationDto>();
        CreateMap<UpdateFeeConfigurationDto, FeeConfiguration>();

        CreateMap<RiskFactorConfiguration, RiskFactorDto>();
        CreateMap<RiskFactorConfiguration, CreateRiskFactorDto>();
        CreateMap<CreateRiskFactorDto, RiskFactorConfiguration>();
        CreateMap<RiskFactorConfiguration, UpdateRiskFactorDto>();
        CreateMap<UpdateRiskFactorDto, RiskFactorConfiguration>();

        // broker mappings
        CreateMap<Broker, BrokerDto>();
        CreateMap<Broker, BrokerDetailsDto>();
        CreateMap<CreateBrokerDto, Broker>();
        CreateMap<Broker, CreateBrokerDto>();
        CreateMap<UpdateBrokerDto, Broker>();
        CreateMap<Broker, UpdateBrokerDto>();
    }
}
