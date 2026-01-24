using System;

namespace Application.Clients.DTOs.Response;

public class ClientBuildingSummaryDto
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
}
