using System;
using Domain.Models.AppUsers;

namespace Application.Core.Interfaces.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(AppUser user);
}
