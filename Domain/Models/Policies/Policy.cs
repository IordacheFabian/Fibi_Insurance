using System;
using Domain.Models.Buildings;
using Domain.Models.Clients;
using Domain.Models.Metadata;

namespace Domain.Models.Policies;

public enum PolicyStatus
{
    Draft,
    Active, 
    Expired,
    Cancelled
}

public class Policy
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = default!;

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = default!;

    public Guid BuildingId { get; set; }
    public Building Building { get; set; } = default!;

    public Guid CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public PolicyStatus PolicyStatus { get; set; } = PolicyStatus.Draft;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }   

    public decimal BasePremium { get; set; }
    public decimal FinalPremium { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DateOnly? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public List<PolicyAdjustement> PolicyAdjustements { get; set; } = new();
}
