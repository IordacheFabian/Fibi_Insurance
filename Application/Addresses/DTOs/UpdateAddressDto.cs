using System;

namespace Application.Addresses;

public class UpdateAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public Guid CityId { get; set; }
}
