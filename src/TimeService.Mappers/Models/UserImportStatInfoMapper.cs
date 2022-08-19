using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class UserImportStatInfoMapper : IUserImportStatInfoMapper
  {
    public List<UserImportStatInfo> Map(List<UserData> usersData, List<CompanyUserData> companyUsersData)
    {
      List<UserImportStatInfo> result = new();

      foreach (UserData user in usersData.OrderBy(x => x.LastName))
      {
        result.Add(new UserImportStatInfo
        {
          UserData = user,
          CompanyUserData = companyUsersData?.FirstOrDefault(x => x.UserId == user.Id)
        });
      }

      return result;
    }
  }
}
