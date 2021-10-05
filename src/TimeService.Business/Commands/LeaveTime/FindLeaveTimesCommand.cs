using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Response.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
  public class FindLeaveTimesCommand : IFindLeaveTimesCommand
  {
    private readonly ILeaveTimeResponseMapper _leaveTimeResponseMapper;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly ILeaveTimeRepository _repository;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly ILogger<FindLeaveTimesCommand> _logger;
    private readonly IConnectionMultiplexer _cache;

    private async Task<List<UserData>> GetUsersData(List<Guid> userIds, List<string> errors)
    {
      if (userIds == null || !userIds.Any())
      {
        return null;
      }

      RedisValue valueFromCache = await _cache.GetDatabase(Cache.Users).StringGetAsync(userIds.GetRedisCacheHashCode());

      if (valueFromCache.HasValue)
      {
        _logger.LogInformation("UserDatas were taken from the cache.");

        return JsonConvert.DeserializeObject<List<UserData>>(valueFromCache.ToString());
      }

      return await GetUsersDataFromBroker(userIds, errors);
    }

    private async Task<List<UserData>> GetUsersDataFromBroker(List<Guid> userIds, List<string> errors)
    {
      if (userIds == null || !userIds.Any())
      {
        return null;
      }

      string message = "Cannot get users data. Please try again later.";
      string loggerMessage = $"Cannot get users data for specific user ids:'{string.Join(",", userIds)}'.";

      try
      {
        Response<IOperationResult<IGetUsersDataResponse>> response = await _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
            IGetUsersDataRequest.CreateObj(userIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("UserDatas were taken from the service.");

          return response.Message.Body.UsersData;
        }

        _logger.LogWarning(loggerMessage + "Reasons: {Errors}", string.Join("\n", response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, loggerMessage);
      }

      errors.Add(message);

      return null;
    }

    public FindLeaveTimesCommand(
      ILeaveTimeResponseMapper leaveTimeResponseMapper,
      IUserInfoMapper userInfoMapper,
      ILeaveTimeRepository repository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      ILogger<FindLeaveTimesCommand> logger,
      IConnectionMultiplexer cache)
    {
      _leaveTimeResponseMapper = leaveTimeResponseMapper;
      _userInfoMapper = userInfoMapper;
      _repository = repository;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _rcGetUsers = rcGetUsers;
      _logger = logger;
      _cache = cache;
    }

    public async Task<FindResultResponse<LeaveTimeResponse>> Execute(FindLeaveTimesFilter filter)
    {
      if (filter == null)
      {
        throw new ArgumentNullException(nameof(filter));
      }

      var isAuthor = filter.UserId.HasValue && filter.UserId == _httpContextAccessor.HttpContext.GetUserId();

      if (!isAuthor && !_accessValidator.IsAdmin())
      {
        throw new ForbiddenException("Not enough rights.");
      }

      var dbLeaveTimes = _repository.Find(filter, out int totalCount);

      List<string> errors = new();
      List<UserInfo> users = (await GetUsersData(dbLeaveTimes.Select(lt => lt.UserId).ToList(), errors)).Select(_userInfoMapper.Map).ToList();

      return new()
      {
        Status = OperationResultStatusType.FullSuccess,
        TotalCount = totalCount,
        Body = dbLeaveTimes.Select(lt => _leaveTimeResponseMapper.Map(lt, users.FirstOrDefault(u => u.Id == lt.UserId))).ToList(),
        Errors = errors
      };
    }
  }
}
