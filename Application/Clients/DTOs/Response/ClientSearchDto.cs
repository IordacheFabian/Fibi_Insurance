using System;
using Application.Core.util;
using Domain.Models.Clients;

namespace Application.Clients.DTOs.Response;

public class ClientSearchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ClientType ClientType { get; set; }
    public string IdentificationMasked { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // building 
    public ICollection<ClientBuildingSummaryDto> Buildings { get; set; } = new List<ClientBuildingSummaryDto>();

}
