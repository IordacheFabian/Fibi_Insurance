using System;
using API.Controllers.BaseControllers;
using Application.Claims.Command;
using Application.Claims.Query;
using Application.Claims.Request;
using Application.Claims.Response;
using Application.Core.PagedResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ClaimsController : AdminBaseController
{


    [HttpGet("claims")]
    public async Task<ActionResult<PagedResult<ClaimListDto>>> GetClaimsList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetClaimsList.Query
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("claims/{claimId:guid}/approve")]
    public async Task<ActionResult<ClaimDto>> ApproveClaim(Guid claimId, ApproveClaimDto approveClaimDto)
    {
        var result = await Mediator.Send(new ApproveClaim.Command
        {
            ClaimId = claimId,
            ApproveClaimDto = approveClaimDto,
        });

        return Ok(result);
    }

    [HttpPost("claims/{claimId:guid}/reject")]
    public async Task<ActionResult<ClaimDto>> RejectClaim(Guid claimId, RejectClaimDto rejectClaimDto)
    {
        var result = await Mediator.Send(new RejectClaim.Command
        {
            ClaimId = claimId,
            RejectClaimDto = rejectClaimDto,
        });

        return Ok(result);
    }

    [HttpPost("claims/{claimId:guid}/review")]
    public async Task<ActionResult<ClaimDto>> MoveToReview(Guid claimId)
    {
        var result = await Mediator.Send(new MoveToReview.Command
        {
            ClaimId = claimId,
            ApprovedAmount = 0,
        });

        return Ok(result);
    }

    [HttpPost("claims/{claimId:guid}/pay")]
    public async Task<ActionResult<ClaimDto>> PayClaim(Guid claimId)
    {
        var result = await Mediator.Send(new PayClaim.Command
        {
            ClaimId = claimId
        });

        return Ok(result);
    }
}
