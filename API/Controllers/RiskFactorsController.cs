using System;
using API.Controllers.BaseControllers;
using Application.Metadatas.RiskFactors.Command;
using Application.Metadatas.RiskFactors.DTOs.Request;
using Application.Metadatas.RiskFactors.DTOs.Response;
using Application.Metadatas.RiskFactors.Queries;
using Domain.Models.Metadatas;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class RiskFactorsController : AdminBaseController
{
    [HttpGet("risk-factors")]
    public async Task<ActionResult<List<RiskFactorDto>>> GetRiskFactorAsync(
        [FromQuery] bool? isActive,
        [FromQuery] RiskLevel? riskLevel)
    {
        var riskFactors = await Mediator.Send(new GetRiskFactors.Query { IsActive = isActive, RiskLevel = riskLevel });
        
        return Ok(riskFactors);
    }

    [HttpPost("risk-factors")] 
    public async Task<ActionResult<RiskFactorDto>> CreateRiskFactorAsync([FromBody] CreateRiskFactorDto createRiskFactorDto)
    {
        var riskFactor = await Mediator.Send(new CreateRiskFactor.Command { CreateRiskFactorDto = createRiskFactorDto });

        return Ok(riskFactor);
    }

    [HttpPut("risk-factors/{id}")]
    public async Task<ActionResult<RiskFactorDto>> UpdateRiskFactorAsync(Guid id, [FromBody] UpdateRiskFactorDto updateRiskFactorDto)
    {
        var riskFactor = await Mediator.Send(new UpdateRiskFactor.Command { Id = id, UpdateRiskFactorDto = updateRiskFactorDto });

        return Ok(riskFactor);
    }
}
