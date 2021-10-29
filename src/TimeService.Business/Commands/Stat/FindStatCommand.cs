using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Requests.Position;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using LT.DigitalOffice.Models.Broker.Responses.Position;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Business.Commands.Stat.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Validation.Import.Interfaces;
using LT.DigitalOffice.TimeService.Validation.Stat.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Business.Commands.Stat
{
  public class FindStatCommand : IFindStatCommand
  {
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetProjectsUsersRequest> _rcGetProjectsUsers;
    private readonly IRequestClient<IGetDepartmentUsersRequest> _rcGetDepartmentUsers;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly IRequestClient<IGetPositionsRequest> _rcGetPositions;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IStatInfoMapper _statInfoMapper;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly ILogger<FindStatCommand> _logger;
    private readonly IRedisHelper _redisHelper;
    private readonly IResponseCreater _responseCreator;
    private readonly IFindStatFilterValidator _validator;

    #region private methods

    private async Task<List<ProjectData>> GetProjectsAsync(List<Guid> projectsIds, List<string> errors)
    {
      if (projectsIds == null || !projectsIds.Any())
      {
        return null;
      }

      (List<ProjectData> projectsFromCache, int _)  =
        await _redisHelper.GetAsync<(List<ProjectData>, int)>(Cache.Projects, projectsIds.GetRedisCacheHashCode(true));

      if (projectsFromCache != null)
      {
        _logger.LogInformation("Projects were taken from the cache. Projects ids: {projectsIds}", string.Join(", ", projectsIds));

        return projectsFromCache;
      }

      return await GetProjectsThroughBrokerAsync(projectsIds, errors);
    }

    private async Task<List<ProjectData>> GetProjectsThroughBrokerAsync(List<Guid> projectsIds, List<string> errors)
    {
      string messageError = "Cannot get projects info. Please, try again later.";
      const string logError = "Cannot get projects info.";

      if (projectsIds == null || !projectsIds.Any())
      {
        return null;
      }

      try
      {
        Response<IOperationResult<IGetProjectsResponse>> result = await _rcGetProjects.GetResponse<IOperationResult<IGetProjectsResponse>>(
          IGetProjectsRequest.CreateObj(
            projectsIds: projectsIds,
            includeUsers: true));

        if (result.Message.IsSuccess)
        {
          _logger.LogInformation("Projects were taken from the service. Projects ids: {projectsIds}", string.Join(", ", projectsIds));

          return result.Message.Body.Projects;
        }

        _logger.LogWarning(logError + "Errors: {errors}.", string.Join("\n", result.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logError);
      }

      errors.Add(messageError);
      return null;
    }

    private async Task<List<UserData>> GetUsersData(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      List<UserData> usersFromCache = await _redisHelper.GetAsync<List<UserData>>(Cache.Users, usersIds.GetRedisCacheHashCode());

      if (usersFromCache != null)
      {
        _logger.LogInformation("UserDatas were taken from the cache. Users ids: {usersIds}", string.Join(", ", usersIds));

        return usersFromCache;
      }

      return await GetUsersDataFromBrokerAsync(usersIds, errors);
    }

    private async Task<List<UserData>> GetUsersDataFromBrokerAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      string message = "Cannot get users data. Please try again later.";
      string loggerMessage = $"Cannot get users data for specific user ids:'{string.Join(",", usersIds)}'.";

      try
      {
        var response = await _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
            IGetUsersDataRequest.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("UserDatas were taken from the service. Users ids: {usersIds}", string.Join(", ", usersIds));

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

    private List<ProjectUserData> GetProjectsUsers(Guid projectId, int skipCount, int takeCount, out int totalCount, List<string> errors)
    {
      string messageError = "Cannot get projects users info. Please, try again later.";
      const string logError = "Cannot get projects users info.";

      try
      {
        IOperationResult<IGetProjectsUsersResponse> result = _rcGetProjectsUsers.GetResponse<IOperationResult<IGetProjectsUsersResponse>>(
          IGetProjectsUsersRequest.CreateObj(
            projectsIds: new() { projectId },
            skipCount: skipCount,
            takeCount: takeCount)).Result.Message;

        if (result.IsSuccess)
        {
          totalCount = result.Body.TotalCount;

          return result.Body.Users;
        }

        _logger.LogWarning(logError + "Errors: {errors}.", string.Join("\n", result.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logError);
      }

      totalCount = 0;
      errors.Add(messageError);

      return null;
    }

    private async Task<(List<Guid>, int totalCount)> FindDepartmentUsers(
      Guid departmentId,
      int skipCount,
      int takeCount,
      DateTime filterByEntryDate,
      List<string> errors)
    {
      string messageError = "Cannot get department users info. Please, try again later.";
      const string logError = "Cannot get users of department with id: '{id}'.";

      try
      {
        IOperationResult<IGetDepartmentUsersResponse> result =
          (await _rcGetDepartmentUsers.GetResponse<IOperationResult<IGetDepartmentUsersResponse>>(
            IGetDepartmentUsersRequest.CreateObj(departmentId, skipCount, takeCount, filterByEntryDate))).Message;

        if (result.IsSuccess)
        {
          return (result.Body.UsersIds, result.Body.TotalCount);
        }

        _logger.LogWarning(logError + "Errors: {errors}.", departmentId, string.Join("\n", result.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logError, departmentId);
      }

      errors.Add(messageError);

      return (null, 0);
    }

    private async Task<List<PositionData>> GetPositionsAsync(
      List<Guid> usersIds,
      List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      List<PositionData> positions = await _redisHelper.GetAsync<List<PositionData>>(Cache.Positions, usersIds.GetRedisCacheHashCode());

      if (positions != null)
      {
        _logger.LogInformation("Positions for users were taken from cache. Users ids: {usersIds}", string.Join(", ", usersIds));

        return positions;
      }

      return await GetPositionsThroughBrokerAsync(usersIds, errors);
    }

    private async Task<List<PositionData>> GetPositionsThroughBrokerAsync(
      List<Guid> usersIds,
      List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      const string errorMessage = "Can not get positions info. Please try again later.";

      try
      {
        Response<IOperationResult<IGetPositionsResponse>> response = await _rcGetPositions
          .GetResponse<IOperationResult<IGetPositionsResponse>>(
            IGetPositionsRequest.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("Positions were taken from the service. Users ids: {usersIds}", string.Join(", ", usersIds));

          return response.Message.Body.Positions;
        }
        else
        {
          _logger.LogWarning("Errors while getting positions info. Reason: {Errors}",
            string.Join('\n', response.Message.Errors));
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, errorMessage);
      }

      errors.Add(errorMessage);

      return null;
    }

    private async Task<(List<Guid> usersIds,
      List<ProjectData> projectsDatas,
      List<ProjectUserData> projectUsersDatas,
      List<DbWorkTime> dbWorkTimes,
      List<DbLeaveTime> dbLeaveTimes, int totalCount)>
      FindStatByDepartmentId(
        Guid departmentId,
        int skipCount,
        int takeCount,
        int year,
        int month,
        List<string> errors)
    {
      (List<Guid> userIds, int totalCount) = await FindDepartmentUsers(
        departmentId, skipCount, takeCount, new DateTime(year, month, 1), errors);

      List<DbWorkTime> workTimes = await _workTimeRepository.GetAsync(userIds, null, year, month, true);
      List<DbLeaveTime> leaveTimes = await _leaveTimeRepository.GetAsync(userIds, year, month);

      List<ProjectData> projectsDatas = await GetProjectsAsync(workTimes.Select(wt => wt.ProjectId).ToList(), errors);

      return (userIds, projectsDatas, projectsDatas?.SelectMany(p => p.Users).ToList(), workTimes, leaveTimes, totalCount);
    }

    private async Task<(List<Guid> usersIds,
      List<ProjectData> projectsDatas,
      List<ProjectUserData> projectUsersDatas,
      List<DbWorkTime> dbWorkTimes,
      List<DbLeaveTime> dbLeaveTimes,
      int totalCount)>
      FindStatByProjectId(
        Guid projectId,
        int skipCount,
        int takeCount,
        int year,
        int month,
        List<string> errors)
    {
      List<Guid> userIds = GetProjectsUsers(
        projectId,
        skipCount,
        takeCount,
        out int totalCount,
        errors).Select(pu => pu.UserId).ToList();

      List<DbWorkTime> workTimes = await _workTimeRepository.GetAsync(userIds, null, year, month, true);
      List<DbLeaveTime> leaveTimes = await _leaveTimeRepository.GetAsync(userIds, year, month);

      List<ProjectData> projectsDatas = await GetProjectsAsync(workTimes.Select(wt => wt.ProjectId).ToList(), errors);

      return (userIds, projectsDatas, projectsDatas?.SelectMany(p => p.Users).ToList(), workTimes, leaveTimes, totalCount);
    }

    #endregion

    public FindStatCommand(
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetProjectsUsersRequest> rcGetProjectsUsers,
      IRequestClient<IGetDepartmentUsersRequest> rcGetDepartmentUsers,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      IRequestClient<IGetPositionsRequest> rcGetPositions,
      IUserInfoMapper userInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IStatInfoMapper statInfoMapper,
      IWorkTimeRepository workTimeRepository,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      ILogger<FindStatCommand> logger,
      IRedisHelper redisHelper,
      IResponseCreater responseCreator,
      IFindStatFilterValidator validator)
    {
      _rcGetProjects = rcGetProjects;
      _rcGetProjectsUsers = rcGetProjectsUsers;
      _rcGetDepartmentUsers = rcGetDepartmentUsers;
      _rcGetUsers = rcGetUsers;
      _rcGetPositions = rcGetPositions;
      _userInfoMapper = userInfoMapper;
      _projectInfoMapper = projectInfoMapper;
      _statInfoMapper = statInfoMapper;
      _workTimeRepository = workTimeRepository;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _logger = logger;
      _redisHelper = redisHelper;
      _responseCreator = responseCreator;
      _validator = validator;
    }

    public async Task<FindResultResponse<StatInfo>> ExecuteAsync(FindStatFilter filter)
    {
      if (!_validator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreator.CreateFailureFindResponse<StatInfo>(HttpStatusCode.BadRequest, errors);
      }

      int totalCount = 0;
      List<DbWorkTime> dbWorkTimes;
      List<DbLeaveTime> dbLeaveTimes;
      List<ProjectData> projectsDatas;
      List<ProjectUserData> projectUsersDatas;
      List<Guid> usersIds;

      if (filter.DepartmentId.HasValue)
      {
        (usersIds, projectsDatas, projectUsersDatas, dbWorkTimes, dbLeaveTimes, totalCount) =
          await FindStatByDepartmentId(filter.DepartmentId.Value, filter.SkipCount, filter.TakeCount, filter.Year, filter.Month, errors);
      }
      else
      {
        (usersIds, projectsDatas, projectUsersDatas, dbWorkTimes, dbLeaveTimes, totalCount) =
          await FindStatByProjectId(filter.ProjectId.Value, filter.SkipCount, filter.TakeCount, filter.Year, filter.Month, errors);
      }

      Task<List<UserData>> usersTask = GetUsersData(usersIds, errors);
      Task<List<PositionData>> positionsTask = GetPositionsAsync(usersIds, errors);
      Task<DbWorkTimeMonthLimit> limitTask = _workTimeMonthLimitRepository.GetAsync(filter.Year, filter.Month);

      await Task.WhenAll(usersTask, positionsTask, limitTask);

      List<PositionUserData> positions = (await positionsTask)?.SelectMany(p => p.Users).ToList();
      DbWorkTimeMonthLimit monthLimit = await limitTask;

      List<UserInfo> usersInfos = (await usersTask)
        ?.Select(u => _userInfoMapper.Map(u, positions?.FirstOrDefault(p => p.UserId == u.Id))).ToList();
      List<ProjectInfo> projectInfos = projectsDatas?.Select(_projectInfoMapper.Map).ToList();

      DbWorkTimeMonthLimit limit = await limitTask;

      return new FindResultResponse<StatInfo>
      {
        TotalCount = totalCount,
        Body = usersIds.Select(
          id => _statInfoMapper.Map(
            id,
            usersInfos?.FirstOrDefault(u => u.Id == id),
            projectUsersDatas?.FirstOrDefault(pu => pu.UserId == id),
            limit,
            dbWorkTimes.Where(wt => wt.UserId == id).ToList(),
            projectInfos,
            dbLeaveTimes.Where(lt => lt.UserId == id).ToList()))
          .ToList(),
        Errors = errors,
        Status = errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess
      };
    }
  }
}
