using System;
using Domain.Models.Metadatas;
using Domain.Models.Policies;

namespace Domain.Models.Payments;

public class Payment
{
    public Guid Id { get; set; }
    
    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = default!;
    
    public decimal Amount { get; set; }

    public Guid CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public DateTime PaymentDate { get; set; }

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}
