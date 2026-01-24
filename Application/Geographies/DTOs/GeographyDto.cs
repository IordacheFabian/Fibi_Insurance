using System;

namespace Application.Geographies.DTOs;

public class GeographyDto
{
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty; 

    public Guid CountyId { get; set; }
    public string CountyName { get; set; } = string.Empty;

    public Guid CityId { get; set; }
    public string CityName { get; set; } = string.Empty;

}
