using System;
using Application.Core.Interfaces.IRepositories;
using Application.Payments.Response;
using AutoMapper;
using MediatR;

namespace Application.Payments.Query;

public class GetAllPayments
{
    public class Query : IRequest<List<PaymentDto>>
    {
        
    }

    public class Handler(IPaymentRepository paymentRepository, IMapper mapper) : IRequestHandler<Query, List<PaymentDto>>
    {
        public async Task<List<PaymentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var payments = await paymentRepository.GetAllPaymentsAsync(cancellationToken);
            
            var paymentDtos = mapper.Map<List<PaymentDto>>(payments);

            return paymentDtos;
        }
    }
}
