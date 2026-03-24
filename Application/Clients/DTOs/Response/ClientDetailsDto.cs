using System;
using Application.Addresses.DTOs;
using Application.Buildings.DTOs.Response;
using Domain.Models.Clients;

namespace Application.Clients.DTOs.Response;

public class ClientDetailsDto
{
    public Guid Id { get; set; }
    public ClientType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public string IdentificationNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public List<BuildingListDto> Buildings { get; set; } = new List<BuildingListDto>();

}
