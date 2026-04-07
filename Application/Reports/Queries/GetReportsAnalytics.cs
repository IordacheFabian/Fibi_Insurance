using System;
using Application.Core;
using Application.Core.Interfaces.IRepositories;
using Application.Reports.DTOs.Request;
using Application.Reports.DTOs.Response;
using MediatR;

namespace Application.Reports.Queries;

public class GetReportsAnalytics
{
    public class Query : IRequest<ReportsAnalyticsDto>
    {
        public ReportsAnalyticsRequestDto Request { get; set; } = new();
        public Guid? BrokerId { get; set; }
    }

    public class Handler(IReportsRepository reportsRepository) : IRequestHandler<Query, ReportsAnalyticsDto>
    {
        public async Task<ReportsAnalyticsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var to = request.Request.To ?? today;
            var from = request.Request.From ?? new DateOnly(to.Year, to.Month, 1).AddMonths(-11);

            if (to < from)
            {
                throw new BadRequestException("The 'To' date must be greater than or equal to the 'From' date.");
            }

            return await reportsRepository.GetAnalyticsAsync(from, to, request.BrokerId, request.Request.Currency, request.Request.FilterByCurrency, cancellationToken);
        }
    }
}