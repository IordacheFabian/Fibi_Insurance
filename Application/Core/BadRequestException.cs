using System;

namespace Application.Core;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}
