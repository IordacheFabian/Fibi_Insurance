using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.BaseControllers;

[ApiController]
[Route("api/admin")]
public abstract class AdminBaseController : BaseApiController
{
}