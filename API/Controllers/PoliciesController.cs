using System;
using API.Controllers.BaseControllers;
using Application.Policies.DTOs.Command;
using Application.Policies.DTOs.Requests;
using Application.Policies.DTOs.Response;
using Application.Policies.Queries;
using Domain.Models.Policies;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PoliciesController : BrokerBaseController
{
    [HttpGet("policies")]
    public async Task<ActionResult<List<PolicyListItemDto>>> GetPoliciesAsync(
        [FromQuery] Guid? clientId,
        [FromQuery] Guid? brokerId,
        [FromQuery] string? policyStatus,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate
    )
    {
        var policies = await Mediator.Send(new GetPolicies.Query
        {
            ClientId = clientId,
            BrokerId = brokerId,
            PolicyStatus = string.IsNullOrEmpty(policyStatus) ? null : Enum.Parse<PolicyStatus>(policyStatus, true),
            StartDate = startDate,
            EndDate = endDate
        });

        return Ok(policies);
    }

    [HttpGet("policies/{policyId:guid}", Name = "GetPolicyDetailsAsync")]
    public async Task<ActionResult<PolicyDetailsDto>> GetPolicyDetailsAsync(Guid policyId)
    {
        var policy = await Mediator.Send(new GetPolicyDetails.Query { PolicyId = policyId });

        return Ok(policy);
    }

    [HttpPost("policies")]
    public async Task<ActionResult<PolicyDetailsDto>> CreatePolicyDraftAsync(CreatePolicyDraftDto createPolicyDraftDto)
    {
        var policyId = await Mediator.Send(new CreatePolicyDraft.Command { CreatePolicyDraftDto = createPolicyDraftDto });

        return CreatedAtRoute(
        nameof(GetPolicyDetailsAsync),
        new { policyId }
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
}
