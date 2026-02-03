using System;
using Domain.Models.Policies;

namespace Domain.Models.Broker;

public enum BrokerStatus
{
    Active,
    Inactive
}

public class Broker
{
    public Guid Id { get; set; }
    public string BrokerCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public BrokerStatus BrokerStatus { get; set; } = BrokerStatus.Active;
    public decimal? CommissionPrecentage { get; set; }

    public List<Policy> Policies { get; set; } = new(); 
}
