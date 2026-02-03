using System;
using Application.Brokers.DTOs.Response;
using Application.Buildings.DTOs.Response;
using Application.Clients.DTOs.Response;
using Domain.Models.Policies;

namespace Application.Policies.DTOs.Response;

public class PolicyDetailsDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public PolicyStatus PolicyStatus { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public decimal BasePremium { get; set; }
    public decimal FinalPremium { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName  { get; set; } = string.Empty;

    public ClientDetailsDto Client { get; set; } = new ClientDetailsDto();
    public BuildingDetailsDto Building { get; set; } = new BuildingDetailsDto();
    public BrokerDto Broker { get; set; } = new BrokerDto();

    public IReadOnlyList<PolicyAdjustementDto> PolicyAdjustements { get; set; } = Array.Empty<PolicyAdjustementDto>();

    public DateOnly? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

}
