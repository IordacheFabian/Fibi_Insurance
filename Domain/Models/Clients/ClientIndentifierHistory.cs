using System;

namespace Domain.Models.Clients;

public class ClientIndentifierHistory
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string OldIdentificationNumber { get; set; } = string.Empty;
    public string NewIdentificationNumber { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
