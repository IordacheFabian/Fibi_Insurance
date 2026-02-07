using System;
using Domain.Models.Policies;

namespace Domain.Models.Metadatas;

public class Currency
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal ExchangeRateToBase { get; set; }

    public List<Policy> Policies { get; set; } = new();
}
