﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Requests.Position;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Position;
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

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
  public class FindWorkTimesCommand : IFindWorkTimesCommand
  {
    private readonly IBaseFindFilterValidator _validator;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _monthLimitRepository;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly IRequestClient<IGetPositionsRequest> _rcGetPositions;
    private readonly IWorkTimeResponseMapper _workTimeResponseMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IRedisHelper _redisHelper;
    private readonly IResponseCreater _responsCreator;
    private readonly ILogger<FindWorkTimesCommand> _logger;

    #region private methods

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

    private async Task<List<ProjectData>> GetProjects(List<Guid> projectsIds, Guid? userId, List<string> errors)
    {
      if (projectsIds == null || !projectsIds.Any())
      {
        return null;
      }

      (List<ProjectData> projectsFromCache, int _) =
        await _redisHelper.GetAsync<(List<ProjectData>, int)>(Cache.Projects, CreateProjectCacheKey(projectsIds, userId));

      if (projectsFromCache != null)
      {
        _logger.LogInformation("Projects were taken from the cache. Projects ids: {projectsIds}", string.Join(", ", projectsIds));

        return projectsFromCache;
      }

      return await GetProjectsThroughBroker(projectsIds, userId, errors);
    }

    private async Task<List<ProjectData>> GetProjectsThroughBroker(List<Guid> projectsIds, Guid? userId, List<string> errors)
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
            userId: userId,
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

    #endregion

    public FindWorkTimesCommand(
      IBaseFindFilterValidator validator,
      IWorkTimeResponseMapper workTimeResponseMapper,
      IWorkTimeRepository repository,
      IWorkTimeMonthLimitRepository monthLimitRepository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      IRequestClient<IGetPositionsRequest> rcGetPositions,
      ILogger<FindWorkTimesCommand> logger,
      IProjectInfoMapper projectInfoMapper,
      IUserInfoMapper userInfoMapper,
      IRedisHelper redisHelper,
      IResponseCreater responsCreator)
    {
      _validator = validator;
      _workTimeResponseMapper = workTimeResponseMapper;
      _workTimeRepository = repository;
      _monthLimitRepository = monthLimitRepository;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _rcGetProjects = rcGetProjects;
      _rcGetUsers = rcGetUsers;
      _rcGetPositions = rcGetPositions;
      _logger = logger;
      _projectInfoMapper = projectInfoMapper;
      _userInfoMapper = userInfoMapper;
      _redisHelper = redisHelper;
      _responsCreator = responsCreator;
    }

    public async Task<FindResultResponse<WorkTimeResponse>> ExecuteAsync(FindWorkTimesFilter filter)
    {
      var isActhor = filter.UserId.HasValue && filter.UserId == _httpContextAccessor.HttpContext.GetUserId();

      if (!isActhor && !await _accessValidator.IsAdminAsync())
      {
        return _responsCreator.CreateFailureFindResponse<WorkTimeResponse>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(filter, out List<string> errors))
      {
        return _responsCreator.CreateFailureFindResponse<WorkTimeResponse>(HttpStatusCode.BadRequest, errors);
      }

      (List<DbWorkTime> dbWorkTimes, int totalCount) = await _workTimeRepository.FindAsync(filter);

      List<Guid> usersIds = dbWorkTimes.Select(wt => wt.UserId).Distinct().ToList();

      Task<List<ProjectData>> projectsTask = GetProjects(dbWorkTimes.Select(wt => wt.ProjectId).Distinct().ToList(), filter.UserId, errors);
      Task<List<UserData>> usersTask = GetUsersData(usersIds, errors);
      Task<List<PositionData>> positionsTask = GetPositionsAsync(usersIds, errors);
      Task<(List<DbWorkTimeMonthLimit>, int)> limitTask = _monthLimitRepository.FindAsync(
        new()
        {
          Month = filter.Month,
          Year = filter.Year
        });

      await Task.WhenAll(projectsTask, usersTask, positionsTask, limitTask);

      List<PositionData> positions = await positionsTask;
      List<ProjectData> projects = await projectsTask;
      List<UserData> users = await usersTask;
      (List<DbWorkTimeMonthLimit> monthLimits, int _) = await limitTask;

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
              _userInfoMapper.Map(
                users?.FirstOrDefault(u => u.Id == wt.UserId),
                positions?.FirstOrDefault(p => p.Users.Any(u => u.UserId == wt.UserId))?.Users.First(u => u.UserId == wt.UserId)),
              project?.Users.FirstOrDefault(pu => pu.UserId == wt.UserId),
              _projectInfoMapper.Map(project));
          }).ToList(),
        Errors = errors
      };
    }
  }
}
