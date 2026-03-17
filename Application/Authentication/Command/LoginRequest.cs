using System;
using Application.Authentication.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.Interfaces.Security;
using MediatR;

namespace Application.Authentication.Command;

public class LoginRequest
{
    public class Command : IRequest<AuthResponseDto>
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class Handler(
        IAuthorizationRepository authorizationRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator
    ) : IRequestHandler<Command, AuthResponseDto>
    {
        public async Task<AuthResponseDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await authorizationRepository.GetUserAsync(request.Email, cancellationToken);
            if(user == null || !user.isActive)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var passwordValid = passwordHasher.Verify(request.Password, user.PasswordHash);
            if(!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var token = jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
