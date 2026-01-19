using System;
using Application.Clients.DTOs.Response;
using Application.Clients.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ClientsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ClientSearchDto>> GetClientsAsync()
    {
        return HandleResult(await Mediator.Send(new GetClient.Query {}));
    }

    // [HttpGet]
    // public async Task<ActionResult<ClientSearchDto>> GetClients2Async(string cnp)
    // {
    //     return HandleResult(await Mediator.Send(new GetClientDetails.Query { Id = id }));
    // }

}
