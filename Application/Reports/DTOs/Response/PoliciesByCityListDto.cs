using System;

namespace Application.Reports.DTOs.Response;

public class PoliciesByCityListDto
{
    public Guid CityId { get; set; }
    public string CityName { get; set; } = default!;

    public Guid CountyId { get; set; }
    public string CountyName { get; set; } = default!;

    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = default!;

    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = default!;

    public int PoliciesCount { get; set; }
    public decimal FinalPremium { get; set; }
    public decimal FinalPremiumBaseCurrency { get; set; }
}
