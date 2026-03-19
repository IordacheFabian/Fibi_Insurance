using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Payments.Request;
using Application.Payments.Response;
using Domain.Models.Payments;
using Domain.Models.Policies;
using MediatR;

namespace Application.Payments.Command;

public class CreatePayment
{
    public class Command : IRequest<PaymentDto>
    {
        public Guid PolicyId { get; set; }
        public CreatePaymentDto Payment { get; set; } = default!;
    }

    public class Handler(IPaymentRepository paymentRepository) : IRequestHandler<Command, PaymentDto>
    {
        public async Task<PaymentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var policy = await paymentRepository.GetPolicyByIdAsync(request.PolicyId, cancellationToken);

            if(policy == null)
            {
                throw new NotFoundException("Policy not found");
            }

            if(policy.PolicyStatus != PolicyStatus.Active)
            {
                throw new Exception("Payments can only be made for active policies");
            }

            var currencyExists = await paymentRepository.GetCurrencyByIdAsync(request.Payment.CurrencyId, cancellationToken);

            if(!currencyExists)
            {
                throw new NotFoundException("Currency not found or inactive");
            }

            if(!Enum.TryParse<PaymentMethod>(request.Payment.Method, true, out var method))
            {
                throw new BadRequestException("Invalid payment method");
            }

            if(!Enum.TryParse<PaymentStatus>(request.Payment.Status, true, out var status))
            {
                throw new BadRequestException("Invalid payment status");
            }

            var activeVersion = policy.PolicyVersions.FirstOrDefault(x => x.IsActiveVersion);
            if(activeVersion == null)
            {
                throw new NotFoundException("Active policy version not found");
            }

            var completedAlreadyPaid = await paymentRepository.GetCompletedPaidAmountAsync(request.PolicyId, cancellationToken);
            if(status == PaymentStatus.Completed && completedAlreadyPaid + request.Payment.Amount > activeVersion.FinalPremium)
            {
                throw new BadRequestException("Payment amount exceeds the remaining premium");
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PolicyId = request.PolicyId,
                Amount = request.Payment.Amount,
                CurrencyId = request.Payment.CurrencyId,
                PaymentDate = request.Payment.PaymentDate,
                Method = method,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
            await paymentRepository.AddAsync(payment, cancellationToken);
            var result = await paymentRepository.SaveChangesAsync(cancellationToken);
            if(!result)
            {
                throw new BadRequestException("Failed to create payment");
            }

            return new PaymentDto
            {
                Id = payment.Id,
                PolicyId = payment.PolicyId,
                Amount = payment.Amount,
                CurrencyId = payment.CurrencyId,
                PaymentDate = payment.PaymentDate,
                Method = payment.Method.ToString(),
                Status = payment.Status.ToString()
            };
        }
    }
}
