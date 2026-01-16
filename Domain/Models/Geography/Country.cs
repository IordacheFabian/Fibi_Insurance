using System;

namespace Domain.Models;

public class Country
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // nav props
    public ICollection<County> Counties { get; set; } = [];
}
