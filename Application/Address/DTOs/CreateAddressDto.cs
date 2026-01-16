using System;

namespace Application.Address.DTOs;

public class CreateAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public Guid CityId { get; set; }
}
