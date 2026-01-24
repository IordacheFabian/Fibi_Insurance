using System;

namespace Application.Core;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

}
