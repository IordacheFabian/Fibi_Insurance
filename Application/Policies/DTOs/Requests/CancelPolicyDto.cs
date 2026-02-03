using System;

namespace Application.Policies.DTOs.Requests;

public class CancelPolicyDto
{
    public DateOnly CancellationDate { get; set; } 
    public string CancellationReason { get; set; } = default!;
}
