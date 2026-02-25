using System;

namespace Application.Reports.DTOs.Response;

public class PoliciesByBrokerListDto
{
    public Guid BrokerId { get; set; }
    public string BrokerName { get; set; } = default!;

    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = default!;

    public int PoliciesCount { get; set; }
    public decimal FinalPremium { get; set; }
    public decimal FinalPremiumBaseCurrency { get; set; }
}
