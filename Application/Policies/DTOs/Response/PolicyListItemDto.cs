using System;
using Domain.Models.Policies;

namespace Application.Policies.DTOs.Response;

public class PolicyListItemDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public PolicyStatus PolicyStatus { get; set; }

    public DateOnly StartDate { get; set; } 
    public DateOnly EndDate { get; set; }

    public decimal BasePremium { get; set; }
    public decimal FinalPremium { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;
    public Guid ClientId { get; set; }  
    public Guid BuildingId { get; set; }    
    public Guid BrokerId { get; set; }  
}
