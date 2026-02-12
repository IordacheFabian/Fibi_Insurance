using System;
using API.Controllers.BaseControllers;
using Application.Metadatas.Fees.Queries;
using Application.Metadatas.Fees.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Application.Metadatas.Fees.Command;
using Application.Metadatas.Fees.DTOs.Request;

namespace API.Controllers;

public class FeeConfigurationsController : AdminBaseController
{
    [HttpGet("fees")]
    public async Task<ActionResult<List<FeeConfigurationDto>>> GetFeesAsync([FromQuery] bool? isActive)
    {
        var fees = await Mediator.Send(new GetFees.Query { IsActive = isActive });
        return Ok(fees);
    }

    [HttpPost("fees")]
    public async Task<ActionResult<FeeConfigurationDto>> CreateFeeConfigurationAsync([FromBody] CreateFeeConfigurationDto createFeeConfigurationDto)
    {
        var fee = await Mediator.Send(new CreateFeeConfiguration.Command { CreateFeeConfigurationDto = createFeeConfigurationDto });

        return CreatedAtAction(nameof(GetFeesAsync), new {id = fee.Id }, fee);
    }

    [HttpPut("fees/{feeId:guid}")]
    public async Task<ActionResult> UpdateFeeConfigurationAsync(Guid feeId, [FromBody] UpdateFeeConfigurationDto updateFeeConfigurationDto)
    {
        var fee = await Mediator.Send(new UpdateFeeConfiguration.Command { UpdateDto = updateFeeConfigurationDto });

        return Ok(fee);
    }
}
