using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IUserImportStatInfoMapper
  {
    List<UserImportStatInfo> Map(List<UserData> usersData, List<CompanyUserData> companyUsersData);
  }
}
