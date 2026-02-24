using System;
using API.Controllers.BaseControllers;
using Application.Core.PagedResults;
using Application.Reports.DTOs.Request;
using Application.Reports.DTOs.Response;
using Application.Reports.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ReportsController : AdminBaseController
{
    [HttpGet("policies-by-country")]
    public async Task<ActionResult<PagedResult<PoliciesByCountryListDto>>> GetPoliciesByCountryReport([FromQuery] PoliciesByCountryReportDto reportRequest, int pageNumber = 1, int pageSize = 10)
    {
        var query = new GetPoliciesByCountryReport.Query
        {
            ReportRequest = reportRequest,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        return Ok(result);
    }

    [HttpGet("policies-by-county")]
    public async Task<ActionResult<PagedResult<PoliciesByCountyListDto>>> GetPoliciesByCountyReport([FromQuery] PoliciesByCountyReportDto reportRequest, int pageNumber = 1, int pageSize = 10)
    {
        var query = new GetPoliciesByCountyReport.Query
        {
            ReportRequest = reportRequest,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await Mediator.Send(query);

        return Ok(result);
    }
}
