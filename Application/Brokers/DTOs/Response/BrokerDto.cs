using System;
using Application.Brokers.DTOs.Request;

namespace Application.Brokers.DTOs.Response;

public class BrokerDto : BrokerCreateDto
{
    public Guid Id { get; set; }

}
