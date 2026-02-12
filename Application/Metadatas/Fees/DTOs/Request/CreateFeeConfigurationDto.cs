using System;
using Domain.Models.Metadatas;

namespace Application.Metadatas.Fees.DTOs.Request;

public class CreateFeeConfigurationDto
{
    public string Name { get; set; } = null!;
    public FeeType FeeType { get; set; }
    public decimal Percentage { get; set; }
    public DateOnly EffectiveFrom { get; set; } 
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
