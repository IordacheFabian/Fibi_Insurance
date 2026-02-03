using System;

namespace Application.Policies.DTOs.Requests;

public class CreatePolicyDraftDto
{
    public Guid ClientId { get; set; }
    public Guid BuildingId { get; set; }
    public Guid CurrencyId { get; set; }

    public decimal BasePremium { get; set; }
    public DateOnly StartDate { get; set; } 
    public DateOnly EndDate { get; set; }

    public Guid BrokerId { get; set; }
}
