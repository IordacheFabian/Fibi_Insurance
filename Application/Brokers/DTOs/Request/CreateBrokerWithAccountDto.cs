using Domain.Models.Brokers;

namespace Application.Brokers.DTOs.Request;

public class CreateBrokerWithAccountDto
{
    public string BrokerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public BrokerStatus BrokerStatus { get; set; } = BrokerStatus.Active;
    public decimal? CommissionPercentage { get; set; }
    public string Password { get; set; } = string.Empty;
}