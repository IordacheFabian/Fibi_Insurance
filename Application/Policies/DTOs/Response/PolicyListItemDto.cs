using System;
using Domain.Models.Policies;

namespace Application.Policies.DTOs.Response;

public class PolicyListItemDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = default!;
    public PolicyStatus PolicyStatus { get; set; }

    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = default!;

    public Guid BuildingId { get; set; }
    public string BuildingStreet { get; set; } = default!;
    public string BuildingNumber { get; set; } = default!;
    public string CityName { get; set; } = default!;

    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = default!;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public decimal BasePremium { get; set; }
    public decimal FinalPremium { get; set; }

}
