using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface ICompanyUserInfoMapper
  {
    CompanyUserInfo Map(CompanyUserData companyUserData);
  }
}
