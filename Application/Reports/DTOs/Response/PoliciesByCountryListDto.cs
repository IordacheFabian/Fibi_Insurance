using System;

namespace Application.Reports.DTOs.Response;

public class PoliciesByCountryListDto
{   
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = null!;

    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;

    public int PoliciesCount { get; set; }  
    public decimal FinalPremium { get; set; }
}
