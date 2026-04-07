using System;
using System.Security.Claims;
using API.Controllers.BaseControllers;
using Application.Reports.DTOs.Request;
using Application.Reports.DTOs.Response;
using Application.Reports.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BrokerReportsController : BrokerBaseController
{
    [HttpGet("reports/analytics")]
    public async Task<ActionResult<ReportsAnalyticsDto>> GetAnalyticsAsync([FromQuery] ReportsAnalyticsRequestDto request)
    {
        var brokerIdValue = User.FindFirst("brokerId")?.Value;
        if (!Guid.TryParse(brokerIdValue, out var brokerId))
        {
            return BadRequest(new { message = "Broker context is missing from the authenticated user." });
        }

        var result = await Mediator.Send(new GetReportsAnalytics.Query
        {
            Request = request,
            BrokerId = brokerId,
        });

        return Ok(result);
    }
}