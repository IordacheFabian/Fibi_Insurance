using System;

namespace Application.Payments.Request;

public class CreatePaymentDto
{
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Method { get; set; } = default!;
    public string Status { get; set; } = default!;
}
