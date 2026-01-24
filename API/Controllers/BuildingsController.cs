using System;
using Application.Buildings.Commands;
using Application.Buildings.DTOs.Request;
using Application.Buildings.DTOs.Response;
using Application.Buildings.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


public class BuildingsController : BaseApiController
{
    [HttpGet("clients/{clientId:guid}/buildings")]
    public async Task<ActionResult<List<BuildingListDto>>> GetBuildingsListAsync(Guid clientId)
    {
        return HandleResult(await Mediator.Send(new GetBuildingsList.Query { ClientId = clientId }));
    }

    [HttpGet("buildings/{buildingId:guid}")]
    public async Task<ActionResult<BuildingDetailsDto>> GetBuildingDetailsAsync(Guid buildingId)
    {
        return HandleResult(await Mediator.Send(new GetBuildingDetails.Query { Id = buildingId }));
    }

    [HttpPost("clients/{clientId:guid}/buildings")]
    public async Task<ActionResult<string>> CreateBuildingAsync(Guid clientId, CreateBuildingDto buildingDto)
    {
        var buildingId = await Mediator.Send(new CreateBuilding.Command
        {
            ClientId = clientId,
            BuildingDto = buildingDto   
        });
        
        return CreatedAtAction(nameof(GetBuildingDetails), new { buildingId }, buildingId);
    }

    [HttpPut("buildings/{buildingId:guid}")]
    public async Task<ActionResult> UpdateBuildingAsync(Guid buildingId, UpdateBuildingDto buildingDto)
    {
        buildingDto.Id = buildingId;
        return HandleResult(await Mediator.Send(new UpdateBuilding.Command { BuildingDto = buildingDto }));
    }
}
