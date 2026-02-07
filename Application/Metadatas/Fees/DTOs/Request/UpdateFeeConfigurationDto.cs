using System;

namespace Application.Metadatas.Fees.DTOs.Request;

public class UpdateFeeConfigurationDto
{
    public decimal Percentage { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
