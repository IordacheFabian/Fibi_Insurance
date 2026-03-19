using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Payments.Response;
using MediatR;

namespace Application.Payments.Query;

public class GetPolicyPayments
{
    public class Query : IRequest<List<PaymentDto>>
    {
        public Guid PolicyId { get; set; }
    }

    public class Handler(IPaymentRepository paymentRepository) : IRequestHandler<Query, List<PaymentDto>>
    {
        public async Task<List<PaymentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var policyExists = await paymentRepository.PolicyExistsAsync(request.PolicyId, cancellationToken);
            if(!policyExists)
            {
                throw new NotFoundException("Policy not found");
            }

            return await paymentRepository.GetPaymentsByPolicyIdAsync(request.PolicyId, cancellationToken)
                .ContinueWith(t => t.Result.Select(payment => new PaymentDto
                {
                    Id = payment.Id,
                    PolicyId = payment.PolicyId,
                    Amount = payment.Amount,
                    CurrencyId = payment.CurrencyId,
                    PaymentDate = payment.PaymentDate,
                    Method = payment.Method.ToString(),
                    Status = payment.Status.ToString()
                }).ToList(), cancellationToken);
        }
    }
}
