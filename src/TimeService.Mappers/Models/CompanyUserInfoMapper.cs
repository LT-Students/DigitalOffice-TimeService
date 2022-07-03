using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  internal class CompanyUserInfoMapper : ICompanyUserInfoMapper
  {
    public CompanyUserInfo Map(CompanyUserData companyUserData)
    {
      if (companyUserData is null)
      {
        return null;
      }

      return new()
      {
        Rate = companyUserData.Rate,
        StartWorkingAt = companyUserData.StartWorkingAt
      };
    }
  }
}
