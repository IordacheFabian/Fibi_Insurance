using System;
using Application.Brokers.DTOs.Request;
using Domain.Models.Brokers;

namespace Application.Brokers.DTOs.Response;

public class BrokerDto 
{
    public Guid Id { get; set; }
    public string BrokerCode { get; set; } = default!;
    public string Name { get; set; } = null!;
    public BrokerStatus BrokerStatus { get; set; }
}
