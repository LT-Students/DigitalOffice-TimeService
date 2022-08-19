using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Company;

namespace LT.DigitalOffice.TimeService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface ICompanyService
  {
    Task<List<CompanyData>> GetCompaniesDataAsync(List<Guid> usersIds, List<string> errors);
  }
}
