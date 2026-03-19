using System;

namespace Application.Payments.Response;

public class PaymentDto
{
    public Guid Id { get; set; }    
    public Guid PolicyId { get; set; }
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Method { get; set; } = default!;
    public string Status { get; set; } = default!;
}

