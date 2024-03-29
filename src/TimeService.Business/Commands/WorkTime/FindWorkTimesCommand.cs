﻿using System;
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
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Response.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
  public class FindWorkTimesCommand : IFindWorkTimesCommand
  {
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _monthLimitRepository;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProjectService _projectService;
    private readonly IUserService _userService;
    private readonly IWorkTimeResponseMapper _workTimeResponseMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IResponseCreator _responseCreator;

    public FindWorkTimesCommand(
      IWorkTimeResponseMapper workTimeResponseMapper,
      IWorkTimeRepository repository,
      IWorkTimeMonthLimitRepository monthLimitRepository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IProjectService projectService,
      IUserService userService,
      IProjectInfoMapper projectInfoMapper,
      IUserInfoMapper userInfoMapper,
      IResponseCreator responseCreator)
    {
      _workTimeResponseMapper = workTimeResponseMapper;
      _workTimeRepository = repository;
      _monthLimitRepository = monthLimitRepository;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _projectService = projectService;
      _userService = userService;
      _projectInfoMapper = projectInfoMapper;
      _userInfoMapper = userInfoMapper;
      _responseCreator = responseCreator;
    }

    public async Task<FindResultResponse<WorkTimeResponse>> ExecuteAsync(FindWorkTimesFilter filter)
    {
      bool isAuthor = filter.UserId.HasValue && filter.UserId == _httpContextAccessor.HttpContext.GetUserId();

      if (!isAuthor && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        return _responseCreator.CreateFailureFindResponse<WorkTimeResponse>(HttpStatusCode.Forbidden);
      }

      List<string> errors = new();

      (List<DbWorkTime> dbWorkTimes, int totalCount) = await _workTimeRepository.FindAsync(filter);

      List<Guid> usersIds = dbWorkTimes.Select(wt => wt.UserId).Distinct().ToList();
      usersIds.AddRange(dbWorkTimes.Where(wt => wt.ManagerWorkTime != null).Select(wt => wt.ManagerWorkTime.ModifiedBy.Value).ToList());

      Task<List<ProjectData>> projectsTask = _projectService.GetProjectsDataAsync(
        projectsIds: dbWorkTimes.Select(wt => wt.ProjectId).Distinct().ToList(),
        usersIds: filter.UserId.HasValue ? new() { filter.UserId.Value } : null);
      Task<List<UserData>> usersTask = _userService.GetUsersDataAsync(usersIds, errors);
      Task<(List<DbWorkTimeMonthLimit>, int)> limitTask = _monthLimitRepository.FindAsync(
        new()
        {
          Month = filter.Month,
          Year = filter.Year
        });

      await Task.WhenAll(projectsTask, usersTask, limitTask);

      List<ProjectData> projects = await projectsTask;
      List<UserData> users = await usersTask;
      (List<DbWorkTimeMonthLimit> monthLimits, int _) = await limitTask;

      return new()
      {
        TotalCount = totalCount,
        Body = dbWorkTimes.Select(wt => _workTimeResponseMapper.Map(
          dbWorkTime: wt,
          dbMonthLimit: monthLimits.FirstOrDefault(p => p.Year == wt.Year && p.Month == wt.Month),
          userInfo: _userInfoMapper.Map(
            userData: users?.FirstOrDefault(u => u.Id == wt.UserId)),
          managerInfo: _userInfoMapper.Map(
            userData: users?.FirstOrDefault(u => u.Id == wt.ManagerWorkTime?.ModifiedBy)),
          project: _projectInfoMapper.Map(projects?.FirstOrDefault(p => p.Id == wt.ProjectId)))).ToList(),
        Errors = errors
      };
    }
  }
}
