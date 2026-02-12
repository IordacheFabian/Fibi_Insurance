using System;

namespace Application.Metadatas.Fees.DTOs.Request;

public class UpdateFeeConfigurationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Percentage { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
