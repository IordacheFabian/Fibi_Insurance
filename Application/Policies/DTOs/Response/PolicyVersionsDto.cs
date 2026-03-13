using System;
using Domain.Models.Metadatas;

namespace Application.Policies.DTOs.Response;

public class PolicyVersionsDto
{
    public Guid Id { get; set; }
    public Guid PolicyId { get; set; }
    public int VersionNumber { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public decimal BasePremium { get; set; }
    public decimal FinalPremium { get; set; }

    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = default!;
}
