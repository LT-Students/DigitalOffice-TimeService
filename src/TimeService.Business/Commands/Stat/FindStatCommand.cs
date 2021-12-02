﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Business.Commands.Stat.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Validation.Stat.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Business.Commands.Stat
{
  public class FindStatCommand : IFindStatCommand
  {
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetProjectsUsersRequest> _rcGetProjectsUsers;
    private readonly IRequestClient<IGetDepartmentUsersRequest> _rcGetDepartmentUsers;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly IRequestClient<IGetCompaniesRequest> _rcGetCompanies;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IStatInfoMapper _statInfoMapper;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly ILogger<FindStatCommand> _logger;
    private readonly IRedisHelper _redisHelper;
    private readonly IResponseCreator _responseCreator;
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

      string loggerMessage = $"Cannot get users data for specific user ids:'{string.Join(",", usersIds)}'.";

      try
      {
        Response<IOperationResult<IGetUsersDataResponse>> response =
          await _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
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

      errors.Add("Cannot get users data. Please try again later.");

      return null;
    }

    private async Task<(List<ProjectUserData>, int totalCount)> GetProjectsUsers(Guid projectId, int skipCount, int takeCount, List<string> errors)
    {
      string messageError = "Cannot get projects users info. Please, try again later.";
      const string logError = "Cannot get projects users info.";

      try
      {
        IOperationResult<IGetProjectsUsersResponse> result =
          (await _rcGetProjectsUsers.GetResponse<IOperationResult<IGetProjectsUsersResponse>>(
          IGetProjectsUsersRequest.CreateObj(
            projectsIds: new() { projectId },
            skipCount: skipCount,
            takeCount: takeCount))).Message;

        if (result.IsSuccess)
        {
          return (result.Body.Users, result.Body.TotalCount);
        }

        _logger.LogWarning(logError + "Errors: {errors}.", string.Join("\n", result.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logError);
      }

      errors.Add(messageError);

      return (null, 0);
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

    private async Task<List<CompanyData>> GetCompaniesAsync(
      List<Guid> usersIds,
      List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      List<CompanyData> companies = await _redisHelper.GetAsync<List<CompanyData>>(Cache.Companies, usersIds.GetRedisCacheHashCode());

      if (companies != null)
      {
        _logger.LogInformation("Companies for users were taken from cache. Users ids: {usersIds}", string.Join(", ", usersIds));

        return companies;
      }

      return await GetCompaniesThroughBrokerAsync(usersIds, errors);
    }

    private async Task<List<CompanyData>> GetCompaniesThroughBrokerAsync(
      List<Guid> usersIds,
      List<string> errors)
    {
      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      const string errorMessage = "Can not get companies info. Please try again later.";

      try
      {
        Response<IOperationResult<IGetCompaniesResponse>> response = await _rcGetCompanies
          .GetResponse<IOperationResult<IGetCompaniesResponse>>(
            IGetCompaniesRequest.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("Companies were taken from the service. Users ids: {usersIds}", string.Join(", ", usersIds));

          return response.Message.Body.Companies;
        }
        else
        {
          _logger.LogWarning("Errors while getting companies info. Reason: {Errors}",
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
      (List<ProjectUserData> users, int totalCount) = (await GetProjectsUsers(
        projectId, skipCount, takeCount, errors));

      List<Guid> usersIds = users.Select(pu => pu.UserId).ToList();

      List<DbWorkTime> workTimes = await _workTimeRepository.GetAsync(usersIds, null, year, month, true);
      List<DbLeaveTime> leaveTimes = await _leaveTimeRepository.GetAsync(usersIds, year, month);

      List<ProjectData> projectsDatas = await GetProjectsAsync(workTimes.Select(wt => wt.ProjectId).ToList(), errors);

      return (usersIds, projectsDatas, projectsDatas?.SelectMany(p => p.Users).ToList(), workTimes, leaveTimes, totalCount);
    }

    #endregion

    public FindStatCommand(
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetProjectsUsersRequest> rcGetProjectsUsers,
      IRequestClient<IGetDepartmentUsersRequest> rcGetDepartmentUsers,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      IRequestClient<IGetCompaniesRequest> rcGetCompanies,
      IUserInfoMapper userInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IStatInfoMapper statInfoMapper,
      IWorkTimeRepository workTimeRepository,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      ILogger<FindStatCommand> logger,
      IRedisHelper redisHelper,
      IResponseCreator responseCreator,
      IFindStatFilterValidator validator)
    {
      _rcGetProjects = rcGetProjects;
      _rcGetProjectsUsers = rcGetProjectsUsers;
      _rcGetDepartmentUsers = rcGetDepartmentUsers;
      _rcGetUsers = rcGetUsers;
      _rcGetCompanies = rcGetCompanies;
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
      Task<List<CompanyData>> companiesTask = GetCompaniesAsync(usersIds, errors);
      Task<DbWorkTimeMonthLimit> limitTask = _workTimeMonthLimitRepository.GetAsync(filter.Year, filter.Month);

      await Task.WhenAll(usersTask, companiesTask, limitTask);

      List<CompanyUserData> companies = (await companiesTask)?.SelectMany(p => p.Users).ToList();
      DbWorkTimeMonthLimit monthLimit = await limitTask;

      List<UserInfo> usersInfos = (await usersTask)
        ?.Select(u => _userInfoMapper.Map(u, companies?.FirstOrDefault(p => p.UserId == u.Id))).ToList();
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
