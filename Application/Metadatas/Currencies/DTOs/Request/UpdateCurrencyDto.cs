using System;

namespace Application.Metadatas.Currencies.DTOs.Request;

public class UpdateCurrencyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal ExchangeRateToBase { get; set; }
    public bool IsActive { get; set; }  
}
