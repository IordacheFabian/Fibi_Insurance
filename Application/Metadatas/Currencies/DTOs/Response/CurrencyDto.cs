using System;

namespace Application.Metadatas.Currencies.DTOs.Response;

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal ExchangeRateToBase { get; set; }
    public bool IsActive { get; set; }
}
