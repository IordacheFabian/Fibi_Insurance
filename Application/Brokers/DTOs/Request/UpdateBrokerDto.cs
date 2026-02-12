using System;
using Domain.Models.Brokers;

namespace Application.Brokers.DTOs.Request;

public class UpdateBrokerDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public decimal? CommissionPercentage { get; set; }
}
