using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Core;
using Microsoft.Extensions.DependencyInjection;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator => 
        _mediator ??= HttpContext.RequestServices.GetService<IMediator>() 
            ?? throw new InvalidOperationException("IMediator service is unvailable");

    protected ActionResult HandlResult<T>(Result<T> result)
    {
        if(!result.IsSuccess && result.Code == 404) return NotFound();

        if(result.IsSuccess && result.Value != null) return Ok(result.Value);

        return BadRequest(result.Error);
    }
}
