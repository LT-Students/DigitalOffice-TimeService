using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;

namespace LT.DigitalOffice.TimeService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IUserService
  {
    Task<List<UserData>> GetUsersDataAsync(List<Guid> usersIds, List<string> errors);

    Task<(List<UserData> usersData, int totalCount)> GetFilteredUsersDataAsync(
      List<Guid> usersIds,
      int skipcount,
      int takecount,
      List<string> errors,
      bool? ascendingSort = null);
  }
}
