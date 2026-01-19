using System;

namespace Application.Addresses.DTOs;

public class AddressDetailsDto
{
    public Guid Id { get; set; } 
    public string CityName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;

    public Guid CityId { get; set; }
}
