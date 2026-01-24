using System;
using System.Runtime.InteropServices;
using Application.Addresses;
using Application.Addresses.DTOs;
using Application.Buildings.DTOs.Request;
using Application.Buildings.DTOs.Response;
using Application.Clients.DTOs;
using Application.Clients.DTOs.Response;
using Application.Core.util;
using Application.Geographies.DTOs;
using AutoMapper;
using Domain.Models;
using Domain.Models.Buildings;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;

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

        CreateMap<Building, BuildingDetailsDto>();
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
    }
}
