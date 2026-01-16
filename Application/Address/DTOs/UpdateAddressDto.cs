using System;

namespace Application.Address;

public class UpdateAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public Guid CityId { get; set; }
}
