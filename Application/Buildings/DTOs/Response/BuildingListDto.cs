using System;
using Application.Buildings.DTOs.Request;
using Domain.Models.Buildings;

namespace Application.Buildings.DTOs.Response;

public class BuildingListDto
{
    public Guid Id { get; set; }
    
    public string Address { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public BuildingType BuildingType { get; set; }
    public int InsuredValue { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
}
