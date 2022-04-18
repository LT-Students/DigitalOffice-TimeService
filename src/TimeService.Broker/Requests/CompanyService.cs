using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Broker.Requests
{
  public class CompanyService : ICompanyService
  {
    private readonly ILogger<CompanyService> _logger;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IRequestClient<IGetCompaniesRequest> _rcGetCompanies;

    public CompanyService(
      ILogger<CompanyService> logger,
      IGlobalCacheRepository globalCache,
      IRequestClient<IGetCompaniesRequest> rcGetCompanies)
    {
      _logger = logger;
      _globalCache = globalCache;
      _rcGetCompanies = rcGetCompanies;
    }

    public async Task<List<CompanyData>> GetCompaniesDataAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return null;
      }

      List<CompanyData> companiesData = await _globalCache.GetAsync<List<CompanyData>>(Cache.Companies, usersIds.GetRedisCacheHashCode());

      if (companiesData is null)
      {
        companiesData =
          (await RequestHandler.ProcessRequest<IGetCompaniesRequest, IGetCompaniesResponse>(
            _rcGetCompanies,
            IGetCompaniesRequest.CreateObj(usersIds),
            errors,
            _logger))
          ?.Companies;
      }

      return companiesData;
    }
  }
}
