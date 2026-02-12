using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Core;
using Microsoft.Extensions.DependencyInjection;

namespace API.Controllers.BaseControllers;

[ApiController]
[Route("api/brokers")]
public abstract class BrokerBaseController : BaseApiController
{
}
