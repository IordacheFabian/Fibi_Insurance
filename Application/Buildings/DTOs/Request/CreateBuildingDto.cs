using System;
using Application.Addresses.DTOs;
using Domain.Models.Buildings;

namespace Application.Buildings.DTOs.Request;

public class CreateBuildingDto
{
    public Guid ClientId { get; set; }
    public CreateAddressDto Address { get; set;  } = default!;

    public int ConstructionYear { get; set; }
    public BuildingType BuildingType { get; set; }
    public int NumberOfFloors { get; set; }
    public int SurfaceArea { get; set; } 
    public int InsuredValue { get; set; }
    public string RiskIndicators { get; set; } = string.Empty;
}
