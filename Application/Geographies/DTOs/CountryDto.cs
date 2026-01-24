using System;

namespace Application.Geographies.DTOs;

public class CountryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
