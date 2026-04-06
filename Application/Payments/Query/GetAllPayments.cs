using System;
using Application.Core.Interfaces.IRepositories;
using Application.Payments.Response;
using MediatR;

namespace Application.Payments.Query;

public class GetAllPayments
{
    public class Query : IRequest<List<PaymentDto>>
    {
        
    }

    public class Handler(IPaymentRepository paymentRepository) : IRequestHandler<Query, List<PaymentDto>>
    {
        public async Task<List<PaymentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var payments = await paymentRepository.GetAllPaymentsAsync(cancellationToken);

            return payments.Select(payment => new PaymentDto
            {
                Id = payment.Id,
                PolicyId = payment.PolicyId,
                PolicyNumber = payment.Policy.PolicyNumber,
                ClientName = payment.Policy.Client.Name,
                Amount = payment.Amount,
                CurrencyId = payment.CurrencyId,
                CurrencyCode = payment.Currency.Code,
                PaymentDate = payment.PaymentDate,
                Method = payment.Method.ToString(),
                Status = payment.Status.ToString()
            }).ToList();
        }
    }
}
