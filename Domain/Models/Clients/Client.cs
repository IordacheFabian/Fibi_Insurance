using System;
using Domain.Models.Brokers;
using Domain.Models.Buildings;
using Domain.Models.Geography.Address;

namespace Domain.Models.Clients;

public enum ClientType
{
    Individual,
    Company,
}

public class Client
{
    public Guid Id { get; set; }

    public Guid? BrokerId { get; set; }
    public Broker? Broker { get; set; }

    public ClientType ClientType { get; set; } = ClientType.Individual;
    public string Name { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty; 

    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    
    // nav props
    public ICollection<Address> Addresses { get; set; } = []; // !!!!
    public ICollection<Building> Buildings { get; set; } = [];
}
