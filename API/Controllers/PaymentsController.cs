using System;
using API.Controllers.BaseControllers;
using Application.Payments.Command;
using Application.Payments.Query;
using Application.Payments.Request;
using Application.Payments.Response;
using Application.Policies.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PaymentsController : BrokerBaseController
{
    [HttpPost("policies/{policyId:guid}/payments")]
    public async Task<ActionResult<PaymentDto>> CreatePaymentAsync(Guid policyId, [FromBody] CreatePaymentDto createPaymentDto)
    {
        var payment = await Mediator.Send(new CreatePayment.Command { PolicyId = policyId, Payment = createPaymentDto });

        return Ok (payment);
    }

    [HttpGet("policies/{policyId:guid}/payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetPaymentsByPolicyIdAsync(Guid policyId)
    {
        var payments = await Mediator.Send(new GetPolicyPayments.Query { PolicyId = policyId });

        return Ok(payments);
    }
}   
