using System;
using Domain.Models.Metadatas;

namespace Application.Metadatas.Fees.DTOs.Response;

public class FeeConfigurationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public FeeType FeeType { get; set; }
    public decimal Percentage { get; set; }
    public DateOnly EffectiveFrom { get; set; } 
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
