using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Response.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
  public class FindLeaveTimesCommand : IFindLeaveTimesCommand
  {
    private readonly ILeaveTimeResponseMapper _leaveTimeResponseMapper;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly ILeaveTimeRepository _repository;
    private readonly ILeaveTimeAccessValidationHelper _leaveTimeAccessValidationHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private readonly IResponseCreator _responsCreator;

    public FindLeaveTimesCommand(
      ILeaveTimeResponseMapper leaveTimeResponseMapper,
      IUserInfoMapper userInfoMapper,
      ILeaveTimeRepository repository,
      ILeaveTimeAccessValidationHelper leaveTimeAccessValidationHelper,
      IHttpContextAccessor httpContextAccessor,
      IUserService userService,
      IResponseCreator responseCreator)
    {
      _leaveTimeResponseMapper = leaveTimeResponseMapper;
      _userInfoMapper = userInfoMapper;
      _repository = repository;
      _leaveTimeAccessValidationHelper = leaveTimeAccessValidationHelper;
      _httpContextAccessor = httpContextAccessor;
      _userService = userService;
      _responsCreator = responseCreator;
    }

    public async Task<FindResultResponse<LeaveTimeResponse>> ExecuteAsync(FindLeaveTimesFilter filter)
    {
      bool isAuthor = filter.UserId.HasValue && filter.UserId == _httpContextAccessor.HttpContext.GetUserId();

      if (!isAuthor && !await _leaveTimeAccessValidationHelper.HasRightsAsync(filter.UserId))
      {
        return _responsCreator.CreateFailureFindResponse<LeaveTimeResponse>(HttpStatusCode.Forbidden);
      }

      List<string> errors = new();

      (List<DbLeaveTime> dbLeaveTimes, int totalCount) = await _repository.FindAsync(filter);

      List<Guid> usersIds = dbLeaveTimes.Select(lt => lt.UserId)
        .Concat(dbLeaveTimes.Where(lt => lt.ManagerLeaveTime != null).Select(lt => lt.ManagerLeaveTime.CreatedBy))
        .Distinct().ToList();

      Task<List<UserData>> usersTask = _userService.GetUsersDataAsync(usersIds, errors);

      List<UserInfo> users = (await usersTask)
        ?.Select(u => _userInfoMapper.Map(u)).ToList();

      return new()
      {
        TotalCount = totalCount,
        Body = dbLeaveTimes.Select(
          lt => _leaveTimeResponseMapper.Map(
            dbLeaveTime: lt,
            user: users?.FirstOrDefault(u => u.Id == lt.UserId),
            manager: lt.ManagerLeaveTime is not null
              ? users.FirstOrDefault(u => u.Id == lt.ManagerLeaveTime.CreatedBy)
              : null))
          .ToList(),
        Errors = errors
      };
    }
  }
}
