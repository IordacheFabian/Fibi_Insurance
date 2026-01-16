using System;

namespace Domain.Models.Geography.Address;

public class Address
{
    public Guid Id { get; set; } 
    public string Street { get; set; } = default!;
    public string Number { get; set; } = default!;

    public Guid CityId { get; set; }
    public City City { get; set; } = default!;

    public bool IsPrimary { get; set; }

}
