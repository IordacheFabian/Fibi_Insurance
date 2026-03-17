using System;
using System.Security.Claims;
using API.Controllers.BaseControllers;
using Application.Authentication.Command;
using Application.Authentication.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    [HttpPost("register-broker")]
    public async Task<ActionResult<AuthResponseDto>> RegisterBroker(RegisterBrokerRequest.Command command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }   

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest.Command command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var brokerId = User.FindFirst("brokerId")?.Value;

        return Ok(new
        {
            userId,
            email,
            role,
            brokerId
        });
    }
}
