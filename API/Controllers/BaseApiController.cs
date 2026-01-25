using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Core;
using Microsoft.Extensions.DependencyInjection;

namespace API.Controllers;

[ApiController]
[Route("api/brokers")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator =>
    _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}
