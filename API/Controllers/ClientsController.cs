using System;
using Application.Clients.Commands;
using Application.Clients.DTOs;
using Application.Clients.DTOs.Response;
using Application.Clients.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ClientsController : BaseApiController
{
    [HttpGet("clients")]
    public async Task<ActionResult<List<ClientSearchDto>>> GetClientsAsync([FromQuery] string? name, [FromQuery] string? identifier)
    {
        return Ok(await Mediator.Send(new GetClient.Query
        {
            Name = name,
            Identifier = identifier
        }));
    }

    [HttpGet("clients/{clientId:guid}")]
    public async Task<ActionResult<ClientDetailsDto>> GetClientDetailsAsync(Guid clientId)
    {
        return Ok(await Mediator.Send(new GetClientDetails.Query { Id = clientId }));
    }

    [HttpPost("clients")]
    public async Task<ActionResult<string>> CreateClientAsync(CreateClientDto clientDto)
    {
        var clientId = await Mediator.Send(new CreateClient.Command { ClientDto = clientDto });
        return CreatedAtAction(nameof(GetClientDetailsAsync), new { id = clientId }, clientId);
    }

    [HttpPut("clients/{clientId:guid}")]
    public async Task<ActionResult> UpdateClientAsync(Guid clientId, UpdateClientDto clientDto)
    {
        await Mediator.Send(new UpdateClient.Command { Id = clientId, ClientDto = clientDto });
        return NoContent();
    }

}
