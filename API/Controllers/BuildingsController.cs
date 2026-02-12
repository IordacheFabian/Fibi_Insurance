using System;
using API.Controllers.BaseControllers;
using Application.Buildings.Commands;
using Application.Buildings.DTOs.Request;
using Application.Buildings.DTOs.Response;
using Application.Buildings.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


public class BuildingsController : BrokerBaseController
{
    [HttpGet("clients/{clientId:guid}/buildings")]
    public async Task<ActionResult<List<BuildingListDto>>> GetBuildingsListAsync(Guid clientId)
    {
        var buildings = await Mediator.Send(new GetBuildingsList.Query { ClientId = clientId });
        return Ok(buildings);
    }

    [HttpGet("buildings/{buildingId:guid}")]
    public async Task<ActionResult<BuildingDetailsDto>> GetBuildingDetailsAsync(Guid buildingId)
    {
        var buildingDetails = await Mediator.Send(new GetBuildingDetails.Query { Id = buildingId });
        return Ok(buildingDetails);
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
        var command = new UpdateBuilding.Command
        {
            BuildingDto = buildingDto
        };
        await Mediator.Send(command);
        return NoContent();
    }
}
