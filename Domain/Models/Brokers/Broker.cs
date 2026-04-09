using System;
using Domain.Models.Clients;
using Domain.Models.Policies;

namespace Domain.Models.Brokers;

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
    public decimal? CommissionPercentage { get; set; }

    public DateTime CreatedAt { get; set; } 
    public DateTime? UpdatedAt { get; set; }

    public List<Client> Clients { get; set; } = new();
    public List<Policy> Policies { get; set; } = new(); 
}
