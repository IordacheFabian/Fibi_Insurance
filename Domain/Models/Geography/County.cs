using System;

namespace Domain.Models;

public class County
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;

    // nav props
    public Country Country { get; set; } = null!;
    public ICollection<City> Cities { get; set; } = [];
}
