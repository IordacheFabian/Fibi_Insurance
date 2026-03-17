using System;
using Domain.Models.Brokers;

namespace Domain.Models.AppUsers;

public class AppUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = default!;

    public Guid? BrokerId { get; set; }
    public Broker? Broker { get; set; }

    public bool isActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
