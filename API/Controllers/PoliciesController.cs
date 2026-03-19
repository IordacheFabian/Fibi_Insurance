using System;
using API.Controllers.BaseControllers;
using Application.Claims.Command;
using Application.Claims.Query;
using Application.Claims.Request;
using Application.Claims.Response;
using Application.Core.PagedResults;
using Application.Policies.Command;
using Application.Policies.DTOs.Requests;
using Application.Policies.DTOs.Response;
using Application.Policies.Queries;
using Domain.Models.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PoliciesController : BrokerBaseController
{
    // [Authorize(Roles = "Broker")]
    [HttpGet("policies")]
    public async Task<ActionResult<PagedResult<PolicyListItemDto>>> GetPoliciesAsync(
        [FromQuery] Guid? clientId,
        [FromQuery] Guid? brokerId,
        [FromQuery] string? policyStatus,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var policies = await Mediator.Send(new GetPolicies.Query
        {
            ClientId = clientId,
            BrokerId = brokerId,
            PolicyStatus = string.IsNullOrEmpty(policyStatus) ? null : Enum.Parse<PolicyStatus>(policyStatus, true),
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        return Ok(policies);
    }

    [HttpGet("policies/{policyId:guid}", Name = "GetPolicyDetailsAsync")]
    public async Task<ActionResult<PolicyDetailsDto>> GetPolicyDetailsAsync(Guid policyId)
    {
        var policy = await Mediator.Send(new GetPolicyDetails.Query { PolicyId = policyId });

        return Ok(policy);
    }

    [HttpGet("policies/{policyId:guid}/endorsements")]
    public async Task<ActionResult<List<PolicyEndorsementsDto>>> GetPolicyEndorsementsAsync(Guid policyId)
    {
        var endorsements = await Mediator.Send(new GetPolicyEndorsementsPolicyId.Query { PolicyId = policyId });

        return Ok(endorsements);
    }

    [HttpGet("policies/{policyId:guid}/versions")]
    public async Task<ActionResult<List<PolicyVersionsDto>>> GetPolicyVersionsAsync(Guid policyId)
    {
        var versions = await Mediator.Send(new GetPolicyVersions.Query { PolicyId = policyId });

        return Ok(versions);
    }

    [HttpGet("policies/{policyId:guid}/claims")]
    public async Task<ActionResult<List<ClaimDto>>> GetPolicyClaimsAsync(Guid policyId)
    {
        var claims = await Mediator.Send(new GetPolicyClaims.Query { PolicyId = policyId });

        return Ok(claims);
    }

    [HttpGet("claims/{claimId:guid}")]
    public async Task<ActionResult<ClaimDto>> GetClaimAsync(Guid claimId)
    {
        var claim = await Mediator.Send(new GetClaim.Query { ClaimId = claimId });

        return Ok(claim);
    }

    [HttpPost("policies")]
    public async Task<ActionResult<PolicyDetailsDto>> CreatePolicyDraftAsync(CreatePolicyDraftDto createPolicyDraftDto)
    {
        var policy = await Mediator.Send(new CreatePolicyDraft.Command { CreatePolicyDraftDto = createPolicyDraftDto });

        return CreatedAtRoute(
            nameof(GetPolicyDetailsAsync),
            new { policyId = policy.Id },
            policy
        );
    }

    [HttpPost("policies/{policyId:guid}/activate")]
    public async Task<ActionResult> ActivatePolicyAsync(Guid policyId)
    {
        await Mediator.Send(new ActivatePolicy.Command { PolicyId = policyId });

        return NoContent();
    }

    [HttpPost("policies/{policyId:guid}/cancel")]
    public async Task<ActionResult> CancelPolicyAsync(Guid policyId, CancelPolicyDto cancelPolicyDto)
    {
        await Mediator.Send(new CancelPolicy.Command
        {
            PolicyId = policyId,
            CancelPolicyDto = cancelPolicyDto
        });

        return NoContent();
    }

    [HttpPost("policies/{policyId:guid}/endorsements")]
    public async Task<ActionResult> CreatePolicyEndorsementAsync(Guid policyId, CreatePolicyEndorsementDto createPolicyEndorsementDto)
    {
        await Mediator.Send(new CreatePolicyEndorsement.Command
        {
            PolicyId = policyId,
            CreatePolicyEndorsementDto = createPolicyEndorsementDto,
            CreatedBy = User.Identity?.Name ?? "Unknown"
        });

        return NoContent();
    }
    
    [HttpPost("policies/{policyId:guid}/claims")]
    public async Task<ActionResult<ClaimDto>> CreateClaimAsync(Guid policyId, CreateClaimDto createClaimDto)
    {
        var result = await Mediator.Send(new CreateClaim.Command
        {
            PolicyId = policyId,
            Claim = createClaimDto,
        });

        return Ok(result);
    }
}
