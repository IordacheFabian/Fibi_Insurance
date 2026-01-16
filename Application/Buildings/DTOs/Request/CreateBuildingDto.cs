using System;
using Application.Address.DTOs;

namespace Application.Buildings.DTOs.Request;

public enum BuildingType
{
    Residential,
    Commercial,
    Industrial,
    MixedUse
}

public class CreateBuildingDto
{
    public Guid ClientId { get; set; }
    public CreateAddressDto Address { get; set;  } = default!;

    public int ConstructionYear { get; set; }
    public BuildingType BuildingType { get; set; }
    public int NumberOfFloors { get; set; }
    public int SurfaceArea { get; set; } 
    public int InsuredValue { get; set; }
}
