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
    Task<List<UserData>> GetUsersDataAsync(List<Guid> usersIds, List<string> errors, bool includeBaseEmail = false);

    Task<(List<UserData> usersData, int totalCount)> GetFilteredUsersDataAsync(
      List<Guid> usersIds,
      int skipCount,
      int takeCount,
      bool? ascendingSort,
      string nameIncludeSubstring,
      bool? isActive,
      List<string> errors);

    Task<List<Guid>> CheckUsersExistenceAsync(List<Guid> usersIds, List<string> errors = null);
  }
}
