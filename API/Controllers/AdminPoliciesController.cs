using System;
using API.Controllers.BaseControllers;
using Application.Claims.Query;
using Application.Claims.Response;
using Application.Core.PagedResults;
using Application.Payments.Query;
using Application.Payments.Response;
using Application.Policies.DTOs.Response;
using Application.Policies.Queries;
using Domain.Models.Policies;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AdminPoliciesController : AdminBaseController
{
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

    [HttpGet("policies/{policyId:guid}")]
    public async Task<ActionResult<PolicyDetailsDto>> GetPolicyDetailsAsync(Guid policyId)
    {
        var policy = await Mediator.Send(new GetPolicyDetails.Query { PolicyId = policyId, BrokerId = null });

        return Ok(policy);
    }

    [HttpGet("policies/{policyId:guid}/endorsements")]
    public async Task<ActionResult<List<PolicyEndorsementsDto>>> GetPolicyEndorsementsAsync(Guid policyId)
    {
        var endorsements = await Mediator.Send(new GetPolicyEndorsementsPolicyId.Query { PolicyId = policyId, BrokerId = null });

        return Ok(endorsements);
    }

    [HttpGet("endorsements")]
    public async Task<ActionResult<List<PolicyEndorsementsDto>>> GetEndorsementsAsync()
    {
        var endorsements = await Mediator.Send(new GetPolicyEndorsementsList.Query { BrokerId = null });

        return Ok(endorsements);
    }

    [HttpGet("policies/{policyId:guid}/versions")]
    public async Task<ActionResult<List<PolicyVersionsDto>>> GetPolicyVersionsAsync(Guid policyId)
    {
        var versions = await Mediator.Send(new GetPolicyVersions.Query { PolicyId = policyId, BrokerId = null });

        return Ok(versions);
    }

    [HttpGet("policies/{policyId:guid}/claims")]
    public async Task<ActionResult<List<ClaimDto>>> GetPolicyClaimsAsync(Guid policyId)
    {
        var claims = await Mediator.Send(new GetPolicyClaims.Query { PolicyId = policyId, BrokerId = null });

        return Ok(claims);
    }

    [HttpGet("policies/{policyId:guid}/payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetPolicyPaymentsAsync(Guid policyId)
    {
        var payments = await Mediator.Send(new GetPolicyPayments.Query { PolicyId = policyId, BrokerId = null });

        return Ok(payments);
    }
}