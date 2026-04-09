using Application.Brokers.DTOs.Request;
using Application.Brokers.DTOs.Response;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Core.Interfaces.Security;
using AutoMapper;
using Domain.Models.AppUsers;
using Domain.Models.Brokers;
using MediatR;

namespace Application.Brokers.Command;

public class CreateBrokerWithAccount
{
    public class Command : IRequest<BrokerDetailsDto>
    {
        public required CreateBrokerWithAccountDto CreateBrokerWithAccountDto { get; set; }
    }

    public class Handler(
        IBrokerRepository brokerRepository,
        IAuthorizationRepository authorizationRepository,
        IPasswordHasher passwordHasher,
        IMapper mapper) : IRequestHandler<Command, BrokerDetailsDto>
    {
        public async Task<BrokerDetailsDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.CreateBrokerWithAccountDto;
            var brokerCode = dto.BrokerCode.Trim().ToUpperInvariant();
            var email = dto.Email.Trim();

            if (await brokerRepository.BrokerCodeExistsAsync(brokerCode, cancellationToken))
            {
                throw new BadRequestException("Broker code already exists");
            }

            if (await authorizationRepository.EmailExistsAsync(email, cancellationToken))
            {
                throw new BadRequestException("Email already exists");
            }

            var broker = mapper.Map<Broker>(dto);
            broker.Id = Guid.NewGuid();
            broker.BrokerCode = brokerCode;
            broker.Email = email;
            broker.CreatedAt = DateTime.UtcNow;

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = passwordHasher.Hash(dto.Password),
                Role = "Broker",
                BrokerId = broker.Id,
                isActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            await brokerRepository.CreateBrokerAsync(broker, cancellationToken);
            await authorizationRepository.AddUserAsync(user, cancellationToken);

            var result = await authorizationRepository.SaveChangesAsync(cancellationToken);
            if (!result)
            {
                throw new BadRequestException("Failed to create broker and account");
            }

            return mapper.Map<BrokerDetailsDto>(broker);
        }
    }
}