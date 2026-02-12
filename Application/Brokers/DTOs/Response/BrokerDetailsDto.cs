using System;
using Domain.Models.Brokers;

namespace Application.Brokers.DTOs.Response;

public class BrokerDetailsDto
{
    public Guid Id { get; set; }
    public string BrokerCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public BrokerStatus BrokerStatus { get; set; }
    public decimal? CommissionPercentage { get; set; }
}
