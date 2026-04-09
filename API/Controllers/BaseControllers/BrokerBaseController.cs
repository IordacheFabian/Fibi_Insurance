using System;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers.BaseControllers;

[ApiController]
[Route("api/brokers")]
[Authorize(Roles = "Broker")]
public abstract class BrokerBaseController : BaseApiController
{
	protected Guid CurrentBrokerId
	{
		get
		{
			var brokerIdValue = User.FindFirst("brokerId")?.Value;
			if (!Guid.TryParse(brokerIdValue, out var brokerId))
			{
				throw new BadRequestException("Broker context is missing from the authenticated user.");
			}

			return brokerId;
		}
	}
}
