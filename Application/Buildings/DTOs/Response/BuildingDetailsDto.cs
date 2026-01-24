using System;
using Application.Addresses.DTOs;
using Application.Buildings.DTOs.Request;
using Application.Clients.DTOs.Response;
using Application.Geographies.DTOs;

namespace Application.Buildings.DTOs.Response;

public class BuildingDetailsDto
{
    public Guid Id { get; set; }
    public AddressDetailsDto Address { get; set; } = default!;
    
    public ClientDetailsDto Owner { get; set; } = default!;

    public GeographyDto Geography { get; set; } = default!;

    public int CounstructionYear { get; set; }
    public BuildingType BuildingType { get; set; }
    public int NumberOfFloors { get; set; }
    public int SurfaceArea { get; set; }
    public int InsuredValue { get; set; }
    public string RiskIndicatiors { get; set; } = string.Empty;
}
