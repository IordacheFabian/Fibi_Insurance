using System;
using API.Controllers.BaseControllers;
using Application.Brokers.Command;
using Application.Brokers.DTOs.Request;
using Application.Brokers.DTOs.Response;
using Application.Brokers.Queries;
using Application.Core.PagedResults;
using Application.Payments.Query;
using Application.Payments.Response;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BrokersController : AdminBaseController
{
    [HttpGet("brokers")]
    public async Task<ActionResult<PagedResult<BrokerDto>>> GetBrokersAsync([FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var brokers = await Mediator.Send(new GetBrokers.Query { IsActive = isActive, PageNumber = pageNumber, PageSize = pageSize });

        return Ok(brokers);
    }

    [HttpGet("brokers/{id:guid}")]
    public async Task<ActionResult<BrokerDetailsDto>> GetBrokerAsync(Guid id)
    {
        var broker = await Mediator.Send(new GetBrokerDetails.Query { Id = id });

        return Ok(broker);
    }

    [HttpGet("payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetAllPaymentsAsync()
    {
        var payments = await Mediator.Send(new GetAllPayments.Query());

        return Ok(payments);
    }

    [HttpPost("brokers")]
    public async Task<ActionResult<BrokerDetailsDto>> CreateBrokerAsync([FromBody] CreateBrokerDto createBrokerDto)
    {
        var broker = await Mediator.Send(new CreateBroker.Command { CreateBrokerDto = createBrokerDto });

        return CreatedAtAction(nameof(GetBrokersAsync), broker);
    }

    [HttpPost("brokers/registration")]
    public async Task<ActionResult<BrokerDetailsDto>> CreateBrokerWithAccountAsync([FromBody] CreateBrokerWithAccountDto createBrokerWithAccountDto)
    {
        var broker = await Mediator.Send(new CreateBrokerWithAccount.Command
        {
            CreateBrokerWithAccountDto = createBrokerWithAccountDto
        });

        return CreatedAtAction(nameof(GetBrokerAsync), new { id = broker.Id }, broker);
    }

    [HttpPut("brokers/{id:guid}")]
    public async Task<ActionResult> UpdateBrokerAsync(Guid id, [FromBody] UpdateBrokerDto updateBrokerDto)
    {
        var broker = await Mediator.Send(new UpdateBroker.Command { Id = id, UpdateBrokerDto = updateBrokerDto });

        return Ok(broker);
    }

    [HttpPost("brokers/{id:guid}/activate")]
    public async Task<ActionResult<BrokerDto>> ActivateBrokerAsync(Guid id)
    {
        var broker = await Mediator.Send(new ActivateBroker.Command { Id = id });

        return Ok(broker);
    }

    [HttpPost("brokers/{id:guid}/deactivate")]
    public async Task<ActionResult<BrokerDto>> DeactivateBrokerAsync(Guid id)
    {
        var broker = await Mediator.Send(new DeactivateBroker.Command { Id = id });

        return Ok(broker);  
    }
}
