using System;

namespace Application.Core.Interfaces.Security;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
