using System;
using Application.Address;

namespace Application.Buildings.DTOs.Request;

public class UpdateBuildingDto
{
    public UpdateAddressDto? Address { get; set; }

    public int ConstructionYear { get; set; }
    public BuildingType BuildingType { get; set; }
    public int NumberOfFloors { get; set; }
    public int SurfaceArea { get; set; }
    public int InsuredValue { get; set; }
}
