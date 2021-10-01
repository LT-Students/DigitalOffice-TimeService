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
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Response.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
  public class FindWorkTimesCommand : IFindWorkTimesCommand
  {
    private readonly IWorkTimeResponseMapper _workTimeResponseMapper;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _monthLimitRepository;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly ILogger<FindWorkTimesCommand> _logger;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IConnectionMultiplexer _cache;

    private string CreateProjectCacheKey(List<Guid> projectIds, Guid? userId, bool includeUsers = true)
    {
      List<Guid> ids = new();

      if (projectIds != null && projectIds.Any())
      {
        ids.AddRange(projectIds);
      }

      if (userId.HasValue)
      {
        ids.Add(userId.Value);
      }

      return ids.GetRedisCacheHashCode(includeUsers);
    }

    private async Task<List<ProjectData>> GetProjects(List<Guid> projectIds, Guid? userId, List<string> errors)
    {
      if (projectIds == null || !projectIds.Any())
      {
        return null;
      }

      RedisValue projectsFromCache = await _cache.GetDatabase(Cache.Projects).StringGetAsync(CreateProjectCacheKey(projectIds, userId));

      if (projectsFromCache.HasValue)
      {
        (List<ProjectData> projects, int _) = JsonConvert.DeserializeObject<(List<ProjectData>, int)>(projectsFromCache);

        return projects;
      }

      return await GetProjectsThroughBroker(projectIds, userId, errors);
    }

    private async Task<List<ProjectData>> GetProjectsThroughBroker(List<Guid> projectIds, Guid? userId, List<string> errors)
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
            userId: userId,
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
        var response = await _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
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

    public FindWorkTimesCommand(
      IWorkTimeResponseMapper workTimeResponseMapper,
      IWorkTimeRepository repository,
      IWorkTimeMonthLimitRepository monthLimitRepository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      ILogger<FindWorkTimesCommand> logger,
      IProjectInfoMapper projectInfoMapper,
      IUserInfoMapper userInfoMapper,
      IConnectionMultiplexer cache)
    {
      _workTimeResponseMapper = workTimeResponseMapper;
      _workTimeRepository = repository;
      _monthLimitRepository = monthLimitRepository;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _rcGetProjects = rcGetProjects;
      _rcGetUsers = rcGetUsers;
      _logger = logger;
      _projectInfoMapper = projectInfoMapper;
      _userInfoMapper = userInfoMapper;
      _cache = cache;
    }

    public async Task<FindResultResponse<WorkTimeResponse>> Execute(FindWorkTimesFilter filter)
    {
      if (filter == null)
      {
        throw new ArgumentNullException(nameof(filter));
      }

      var isActhor = filter.UserId.HasValue && filter.UserId == _httpContextAccessor.HttpContext.GetUserId();

      if (!isActhor && !_accessValidator.IsAdmin())
      {
        throw new ForbiddenException("Not enough rights.");
      }

      List<string> errors = new();

      var dbWorkTimes = _workTimeRepository.Find(filter, out int totalCount);

      List<ProjectData> projects = await GetProjects(dbWorkTimes.Select(wt => wt.ProjectId).Distinct().ToList(), filter.UserId, errors);
      List<UserData> users = await GetUsersData(dbWorkTimes.Select(wt => wt.UserId).Distinct().ToList(), errors);

      List<DbWorkTimeMonthLimit> monthLimits = _monthLimitRepository.Find(
        new()
        {
          Month = filter.Month,
          Year = filter.Year
        });

      return new()
      {
        Status = errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess,
        TotalCount = totalCount,
        Body = dbWorkTimes.Select(
          wt =>
          {
            ProjectData project = projects?.FirstOrDefault(p => p.Id == wt.ProjectId);
            return _workTimeResponseMapper.Map(
              wt,
              monthLimits.FirstOrDefault(p => p.Year == wt.Year && p.Month == wt.Month),
              _userInfoMapper.Map(users.FirstOrDefault(u => u.Id == wt.UserId)),
              project?.Users.FirstOrDefault(pu => pu.UserId == wt.UserId),
              _projectInfoMapper.Map(project));
          }).ToList(),
        Errors = errors
      };
    }
  }
}
