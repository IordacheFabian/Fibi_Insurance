using System;

namespace Application.Claims.Request;

public class RejectClaimDto
{
    public string Reason { get; set; } = default!;
}
