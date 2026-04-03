using System;
using Domain.Models.Buildings;
using Domain.Models.Policies;

namespace Domain.Models.Metadatas;

public class Currency
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal ExchangeRateToBase { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } 
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Building> Buildings { get; set; } = new List<Building>();
    public ICollection<Policy> Policies { get; set; } = new List<Policy>();
}
