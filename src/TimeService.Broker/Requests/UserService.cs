using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Broker.Requests
{
  public class UserService : IUserService
  {
    private readonly ILogger<UserService> _logger;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsersData;
    private readonly IRequestClient<IFilteredUsersDataRequest> _rcGetFilteredUsersData;

    private string CreateFilterUsersCacheKey(List<Guid> usersIds, int skipCount, int takeCount, bool? ascendingSort)
    {
      if (ascendingSort is null)
      {
        return usersIds.GetRedisCacheHashCode(skipCount, takeCount);
      }
      
      return usersIds.GetRedisCacheHashCode(skipCount, takeCount, ascendingSort);
    }

    public UserService(
      ILogger<UserService> logger,
      IGlobalCacheRepository globalCache,
      IRequestClient<IGetUsersDataRequest> rcGetUsersData,
      IRequestClient<IFilteredUsersDataRequest> rcGetFilteredUsersData)
    {
      _logger = logger;
      _globalCache = globalCache;
      _rcGetUsersData = rcGetUsersData;
      _rcGetFilteredUsersData = rcGetFilteredUsersData;
    }

    public async Task<List<UserData>> GetUsersDataAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return null;
      }

      List<UserData> usersData = await _globalCache.GetAsync<List<UserData>>(Cache.Users, usersIds.GetRedisCacheHashCode());

      if (usersData is null)
      {
        usersData =
          (await RequestHandler.ProcessRequest<IGetUsersDataRequest, IGetUsersDataResponse>(
            _rcGetUsersData,
            IGetUsersDataRequest.CreateObj(usersIds),
            errors,
            _logger))
          ?.UsersData;
      }

      return usersData;
    }

    public async Task<(List<UserData> usersData, int totalCount)> GetFilteredUsersDataAsync(
      List<Guid> usersIds,
      int skipCount,
      int takeCount,
      bool? ascendingSort,
      List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return default;
      }

      (List<UserData> usersData, int totalCount) = await _globalCache.GetAsync<(List<UserData>, int)>(
        Cache.Users,
        CreateFilterUsersCacheKey(usersIds, skipCount, takeCount, ascendingSort));

      if (usersData is null)
      {
        IFilteredUsersDataResponse response =
          await RequestHandler.ProcessRequest<IFilteredUsersDataRequest, IFilteredUsersDataResponse>(
            _rcGetFilteredUsersData,
            IFilteredUsersDataRequest.CreateObj(
              usersIds,
              skipCount: skipCount,
              takeCount: takeCount,
              ascendingSort: ascendingSort),
            errors,
            _logger);

        usersData = response?.UsersData;
        totalCount = response?.TotalCount ?? default;
      }

      return (usersData, totalCount);
    }
  }
}
