using System;
using Application.Buildings.DTOs.Request;

namespace Application.Buildings.DTOs.Response;

public class BuildingListDto
{
    public Guid Id { get; set; }
    
    public string Address { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public BuildingType BuildingType { get; set; }
    public int InsuredValue { get; set; }
}
