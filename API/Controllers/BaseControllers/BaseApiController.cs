using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.BaseControllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator =>
    _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}