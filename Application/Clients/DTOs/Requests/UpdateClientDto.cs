using System;
using Application.Addresses;
using Domain.Models.Geography.Address;

namespace Application.Clients.DTOs;

public class UpdateClientDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public UpdateAddressDto? Address { get; set; }
    
}
