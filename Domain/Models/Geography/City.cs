using System;
using Domain.Models.Buildings;

namespace Domain.Models;

public class City
{
    public Guid Id { get; set; }
    public Guid CountyId { get; set; }
    public string Name { get; set; } = string.Empty;

    // nav props
    public County County { get; set; } = null!;
    public ICollection<Building> Buildings { get; set; } = [];
}
