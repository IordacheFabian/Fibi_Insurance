using System;
using API.Controllers.BaseControllers;
using Application.Buildings.Commands;
using Application.Buildings.DTOs.Request;
using Application.Buildings.DTOs.Response;
using Application.Buildings.Queries;
using Application.Core.PagedResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


public class BuildingsController : BrokerBaseController
{
    [HttpGet("clients/{clientId:guid}/buildings")]
    public async Task<ActionResult<PagedResult<BuildingListDto>>> GetBuildingsListAsync(Guid clientId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var buildings = await Mediator.Send(new GetBuildingsList.Query { ClientId = clientId, BrokerId = CurrentBrokerId, PageNumber = pageNumber, PageSize = pageSize });
        return Ok(buildings);
    }

    [HttpGet("buildings/{buildingId:guid}", Name = nameof(GetBuildingDetailsAsync))]
    public async Task<ActionResult<BuildingDetailsDto>> GetBuildingDetailsAsync(Guid buildingId)
    {
        var buildingDetails = await Mediator.Send(new GetBuildingDetails.Query { Id = buildingId, BrokerId = CurrentBrokerId });
        return Ok(buildingDetails);
    }

    [HttpPost("clients/{clientId:guid}/buildings")]
    public async Task<ActionResult<BuildingDetailsDto>> CreateBuildingAsync(Guid clientId, CreateBuildingDto buildingDto)
    {
        var buildingId = await Mediator.Send(new CreateBuilding.Command
        {
            ClientId = clientId,
            BrokerId = CurrentBrokerId,
            BuildingDto = buildingDto   
        });
        
        return CreatedAtRoute(nameof(GetBuildingDetailsAsync), new { buildingId }, buildingId);
    }

    [HttpPut("buildings/{buildingId:guid}")]
    public async Task<ActionResult> UpdateBuildingAsync(Guid buildingId, UpdateBuildingDto buildingDto)
    {
        buildingDto.Id = buildingId;
        var command = new UpdateBuilding.Command
        {
            BrokerId = CurrentBrokerId,
            BuildingDto = buildingDto
        };
        await Mediator.Send(command);
        return NoContent();
    }
}
