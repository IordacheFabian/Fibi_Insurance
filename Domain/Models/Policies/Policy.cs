using System;
using Domain.Models.Brokers;
using Domain.Models.Buildings;
using Domain.Models.Clients;
using Domain.Models.Metadatas;

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

    public Guid PolicyVersionId { get; set; }
    public ICollection<PolicyVersion> PolicyVersions { get; set; } = default!;

    public Guid BrokerId { get; set; }
    public Broker Broker { get; set; } = default!;

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = default!;

    public Guid BuildingId { get; set; }
    public Building Building { get; set; } = default!;

    public PolicyStatus PolicyStatus { get; set; }
    public int CurrentVersionNumber { get; set; }
}
