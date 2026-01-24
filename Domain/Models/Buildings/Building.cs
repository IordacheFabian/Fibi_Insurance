using System;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;

namespace Domain.Models.Buildings;

public enum Type 
{
    Residential,
    Commercial,
    Industrial,
    MixedUse
}
public class Building
{
    public Guid Id  { get; set; }

    public Guid ClientId { get; set; }
    public Guid AddressId { get; set; } 

    public int ConstructionYear { get; set; }
    public Type BuildingType { get; set; }
    public int NumberOfFloors { get; set; }
    public int SurfaceArea { get; set; } // in square meters
    public int InsuredValue { get; set; } //local currency
    public string RiskIndicatiors { get; set; } = string.Empty; 



    // nav props
    public Client Client { get; set; } = null!;
    public Address Address { get; set; } = null!;
}
