using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.BaseControllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public abstract class AdminBaseController : BaseApiController
{
}