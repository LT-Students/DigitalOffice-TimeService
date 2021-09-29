using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Business.Commands.Stat.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LT.DigitalOffice.TimeService.Business.Commands.Stat
{
  public class FindStatCommand : IFindStatCommand
  {
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetProjectsUsersRequest> _rcGetProjectsUsers;
    private readonly IRequestClient<IGetDepartmentUsersRequest> _rcGetDepartmentUsers;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IStatInfoMapper _statInfoMapper;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly ILogger<FindStatCommand> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConnectionMultiplexer _cache;

    #region private methods

    private async Task<List<ProjectData>> GetProjects(List<Guid> projectIds, List<string> errors)
    {
      if (projectIds == null || !projectIds.Any())
      {
        return null;
      }

      RedisValue projectsFromCache = await _cache.GetDatabase(Cache.Projects).StringGetAsync(projectIds.GetRedisCacheHashCode(true).ToString());

      if (projectsFromCache.HasValue)
      {
        (List<ProjectData> projects, int _) = JsonConvert.DeserializeObject<(List<ProjectData>, int)>(projectsFromCache);

        return projects;
      }

      return await GetProjectsThroughBroker(projectIds, errors);
    }

    private async Task<List<ProjectData>> GetProjectsThroughBroker(List<Guid> projectIds, List<string> errors)
    {
      string messageError = "Cannot get projects info. Please, try again later.";
      const string logError = "Cannot get projects info.";

      if (projectIds == null || !projectIds.Any())
      {
        return null;
      }

      try
      {
        Response<IOperationResult<IGetProjectsResponse>> result = await _rcGetProjects.GetResponse<IOperationResult<IGetProjectsResponse>>(
          IGetProjectsRequest.CreateObj(
            projectsIds: projectIds,
            includeUsers: true));

        if (result.Message.IsSuccess)
        {
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

    private List<Guid> FindDepartmentUsers(
      Guid departmentId, 
      int skipCount, 
      int takeCount, 
      DateTime filterByEntryDate, 
      out int totalCount, 
      List<string> errors)
    {
      string messageError = "Cannot get department users info. Please, try again later.";
      const string logError = "Cannot get users of department with id: '{id}'.";

      try
      {
        IOperationResult<IGetDepartmentUsersResponse> result = _rcGetDepartmentUsers.GetResponse<IOperationResult<IGetDepartmentUsersResponse>>(
          IGetDepartmentUsersRequest.CreateObj(departmentId, skipCount, takeCount, filterByEntryDate)).Result.Message;

        if (result.IsSuccess)
        {
          totalCount = result.Body.TotalCount;

          return result.Body.UserIds;
        }

        _logger.LogWarning(logError + "Errors: {errors}.", departmentId, string.Join("\n", result.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logError, departmentId);
      }

      errors.Add(messageError);

      totalCount = 0;
      return null;
    }

    private async Task<List<UserData>> GetUsersData(List<Guid> userIds, List<string> errors)
    {
      if (userIds == null || !userIds.Any())
      {
        return null;
      }

      RedisValue valueFromCache = await _cache.GetDatabase(Cache.Users).StringGetAsync(userIds.GetRedisCacheHashCode());

      if (valueFromCache.HasValue)
      {
        return JsonConvert.DeserializeObject<List<UserData>>(valueFromCache.ToString());
      }

      return await GetUsersDataThroughBroker(userIds, errors);
    }

    private async Task<List<UserData>> GetUsersDataThroughBroker(List<Guid> userIds, List<string> errors)
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
      List<Guid> userIds = FindDepartmentUsers(
        departmentId,
        skipCount,
        takeCount,
        new DateTime(year, month, 1),
        out int totalCount,
        errors);

      List<DbWorkTime> workTimes = _workTimeRepository.Find(userIds, null, year, month, true);
      List<DbLeaveTime> leaveTimes = _leaveTimeRepository.Find(userIds, year, month);

      List<ProjectData> projectsDatas = await GetProjects(workTimes.Select(wt => wt.ProjectId).ToList(), errors);

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

      List<DbWorkTime> workTimes = _workTimeRepository.Find(userIds, null, year, month, true);
      List<DbLeaveTime> leaveTimes = _leaveTimeRepository.Find(userIds, year, month);

      List<ProjectData> projectsDatas = await GetProjects(workTimes.Select(wt => wt.ProjectId).ToList(), errors);

      return (userIds, projectsDatas, projectsDatas?.SelectMany(p => p.Users).ToList(), workTimes, leaveTimes, totalCount);
    }

    #endregion

    public FindStatCommand(
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetProjectsUsersRequest> rcGetProjectsUsers,
      IRequestClient<IGetDepartmentUsersRequest> rcGetDepartmentUsers,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      IUserInfoMapper userInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IStatInfoMapper statInfoMapper,
      IWorkTimeRepository workTimeRepository,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      ILogger<FindStatCommand> logger,
      IHttpContextAccessor httpContextAccessor,
      IConnectionMultiplexer cache)
    {
      _rcGetProjects = rcGetProjects;
      _rcGetProjectsUsers = rcGetProjectsUsers;
      _rcGetDepartmentUsers = rcGetDepartmentUsers;
      _rcGetUsers = rcGetUsers;
      _userInfoMapper = userInfoMapper;
      _projectInfoMapper = projectInfoMapper;
      _statInfoMapper = statInfoMapper;
      _workTimeRepository = workTimeRepository;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _logger = logger;
      _httpContextAccessor = httpContextAccessor;
      _cache = cache;
    }

    public async Task<FindResultResponse<StatInfo>> Execute(FindStatFilter filter)
    {
      if (!filter.DepartmentId.HasValue && !filter.ProjectId.HasValue
        || filter.DepartmentId.HasValue && filter.ProjectId.HasValue)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new FindResultResponse<StatInfo>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new List<string> { "The request must contain either the id of the department or the project." }
        };
      }

      if (filter.SkipCount < 0)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new FindResultResponse<StatInfo>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new List<string> { "SkipCount cannot be less than 0." }
        };
      }

      if (filter.TakeCount < 1)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new FindResultResponse<StatInfo>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new List<string> { "TakeCount cannot be less than 1." }
        };
      }

      List<string> errors = new();
      int totalCount = 0;

      List<DbWorkTime> dbWorkTimes = null;
      List<DbLeaveTime> dbLeaveTimes = null;
      List<ProjectData> projectsDatas = null;
      List<ProjectUserData> projectUsersDatas = null;
      List<Guid> usersIds = null;

      if (filter.DepartmentId.HasValue)
      {
        (usersIds, projectsDatas, projectUsersDatas, dbWorkTimes, dbLeaveTimes, totalCount) = 
          await FindStatByDepartmentId(filter.DepartmentId.Value, filter.SkipCount, filter.TakeCount, filter.Year, filter.Month, errors);
      }
      else
      {
        (usersIds, projectsDatas, projectUsersDatas, dbWorkTimes, dbLeaveTimes, totalCount) =
          await FindStatByProjectId(filter.DepartmentId.Value, filter.SkipCount, filter.TakeCount, filter.Year, filter.Month, errors);
      }

      List<UserInfo> usersInfos = (await GetUsersData(usersIds, errors)).Select(_userInfoMapper.Map).ToList();
      List<ProjectInfo> projectInfos = projectsDatas?.Select(_projectInfoMapper.Map).ToList();

      return new FindResultResponse<StatInfo>
      {
        TotalCount = totalCount,
        Body = usersIds.Select(
          id => _statInfoMapper.Map(
            id,
            usersInfos?.FirstOrDefault(u => u.Id == id),
            projectUsersDatas?.FirstOrDefault(pu => pu.UserId == id),
            _workTimeMonthLimitRepository.Get(filter.Year, filter.Month),
            dbWorkTimes.Where(wt => wt.UserId == id).ToList(),
            projectInfos,
            dbLeaveTimes.Where(lt => lt.UserId == id).ToList()
            )).ToList(),
        Errors = errors,
        Status = errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess
      };
    }
  }
}
