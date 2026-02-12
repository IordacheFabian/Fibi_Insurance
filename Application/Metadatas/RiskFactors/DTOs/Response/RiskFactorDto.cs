using System;
using Domain.Models.Metadatas;

namespace Application.Metadatas.RiskFactors.DTOs.Response;

public class RiskFactorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;   
    public RiskLevel RiskLevel { get; set; }
    public Guid ReferenceId { get; set; }
    public decimal AdjustementPercentage { get; set; }
    public bool IsActive { get; set; }

}
