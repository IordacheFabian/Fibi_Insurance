using System;
using Application.Addresses.DTOs;
using Domain.Models.Clients;
using Domain.Models.Geography.Address;

namespace Application.Clients.DTOs;


public class CreateClientDto
{
    public ClientType ClientType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public CreateAddressDto? Address { get; set; }
}
