using System;

namespace Domain.Models.Payments;

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
}
