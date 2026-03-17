using System;
using Application.Authentication.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.Interfaces.Security;
using Domain.Models.AppUsers;
using Domain.Models.Brokers;
using MediatR;

namespace Application.Authentication.Command;

public class RegisterBrokerRequest
{
    public class Command : IRequest<AuthResponseDto>
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public Guid BrokerId { get; set; }
    }

    public class Handler(
        IAuthorizationRepository authorizationRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<Command, AuthResponseDto>
    {
        public async Task<AuthResponseDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var emailExist = await authorizationRepository.EmailExistsAsync(request.Email, cancellationToken);
            if(emailExist)
            {
                throw new BadRequestException("Email already exists");
            }

            var broker = await authorizationRepository.GetBrokerAsync(request.BrokerId, cancellationToken);
            if(broker == null)
            {
                throw new NotFoundException("Broker not found");
            }

            if(broker.BrokerStatus != BrokerStatus.Active)
            {
                throw new BadRequestException("Broker is not active");
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHasher.Hash(request.Password),
                Role = "Broker",
                BrokerId = request.BrokerId,
                isActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await authorizationRepository.AddUserAsync(user, cancellationToken);
            var result = await authorizationRepository.SaveChangesAsync(cancellationToken);
            if(!result)
            {
                throw new BadRequestException("Failed to create user");
            }

            var token = jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token, 
                Email = user.Email,
                Role = user.Role,
            };
        }
    }
}
