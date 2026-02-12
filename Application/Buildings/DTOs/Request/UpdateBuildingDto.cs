using System;
using Application.Addresses;
using Domain.Models.Buildings;

namespace Application.Buildings.DTOs.Request;

public class UpdateBuildingDto
{
    public Guid Id { get; set; }

    public UpdateAddressDto? Address { get; set; }

    public int ConstructionYear { get; set; }
    public BuildingType BuildingType { get; set; }
    public int NumberOfFloors { get; set; }
    public int SurfaceArea { get; set; }
    public int InsuredValue { get; set; }
    public string RiskIndicatiors { get; set; } = string.Empty;
}
