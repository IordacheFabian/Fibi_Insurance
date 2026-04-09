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
        var result = await Mediator.Send(new GetReportsAnalytics.Query
        {
            Request = request,
            BrokerId = CurrentBrokerId,
        });

        return Ok(result);
    }
}