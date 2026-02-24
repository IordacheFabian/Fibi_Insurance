using System;
using Application.Core.PagedResults;
using Application.Reports.DTOs.Request;
using Application.Reports.DTOs.Response;

namespace Application.Core.Interfaces.IRepositories;

public interface IReportsRepository
{
    IQueryable<PoliciesByCountryListDto> GetPoliciesByCountryReport(PoliciesByCountryReportDto reportRequest, CancellationToken cancellationToken);
    IQueryable<PoliciesByCountyListDto> GetPoliciesByCountyReport(PoliciesByCountyReportDto reportRequest, CancellationToken cancellationToken);


}
