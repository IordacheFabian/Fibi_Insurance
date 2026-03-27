using System;
using API.Controllers.BaseControllers;
using Application.Clients.Commands;
using Application.Clients.DTOs;
using Application.Clients.DTOs.Response;
using Application.Clients.Queries;
using Application.Core.PagedResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ClientsController : BrokerBaseController
{
    [HttpGet("clients")]
    public async Task<ActionResult<PagedResult<ClientSearchDto>>> GetClientsAsync([FromQuery] string? name, [FromQuery] string? identifier, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await Mediator.Send(new GetClient.Query
        {
            Name = name,
            Identifier = identifier,
            PageNumber = pageNumber,
            PageSize = pageSize,
        }));
    }

    [HttpGet("clients/{clientId:guid}", Name = "GetClient")]
    public async Task<ActionResult<ClientDetailsDto>> GetClientDetailsAsync(Guid clientId)
    {
        return Ok(await Mediator.Send(new GetClientDetails.Query { Id = clientId }));
    }

    [HttpPost("clients")]
    public async Task<ActionResult<ClientDetailsDto>> CreateClientAsync(CreateClientDto clientDto)
    {
        var clientId = await Mediator.Send(new CreateClient.Command { ClientDto = clientDto });
        return CreatedAtRoute("GetClient", new { clientId }, clientId);
    }

    [HttpPut("clients/{clientId:guid}")]
    public async Task<ActionResult> UpdateClientAsync(Guid clientId, UpdateClientDto clientDto)
    {
        await Mediator.Send(new UpdateClient.Command { Id = clientId, ClientDto = clientDto });
        return NoContent();
    }

}
