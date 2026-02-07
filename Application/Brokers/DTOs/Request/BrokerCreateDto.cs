using System;
using Domain.Models.Brokers;

namespace Application.Brokers.DTOs.Request;

public class BrokerCreateDto
{
    public string BrokerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public BrokerStatus BrokerStatus { get; set; } = BrokerStatus.Active;
    public decimal? CommissionPercentage { get; set; }  
}
