﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.Stat.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Validation.Stat.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.Stat
{
  public class FindStatCommand : IFindStatCommand
  {
    private readonly IDepartmentService _departmentService;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IProjectService _projectService;
    private readonly IPositionService _positionService;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IUserStatInfoMapper _userStatInfoMapper;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly IResponseCreator _responseCreator;
    private readonly IFindStatFilterValidator _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;

    public FindStatCommand(
      IDepartmentService departmentService,
      IUserService userService,
      ICompanyService companyService,
      IProjectService projectService,
      IPositionService positionService,
      IUserInfoMapper userInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IUserStatInfoMapper userStatInfoMapper,
      IWorkTimeRepository workTimeRepository,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      IResponseCreator responseCreator,
      IFindStatFilterValidator validator,
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator)
    {
      _departmentService = departmentService;
      _userService = userService;
      _companyService = companyService;
      _projectService = projectService;
      _positionService = positionService;
      _userInfoMapper = userInfoMapper;
      _projectInfoMapper = projectInfoMapper;
      _userStatInfoMapper = userStatInfoMapper;
      _workTimeRepository = workTimeRepository;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _responseCreator = responseCreator;
      _validator = validator;
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
    }

    public async Task<FindResultResponse<UserStatInfo>> ExecuteAsync(FindStatFilter filter)
    {
      ValidationResult validationResult = await _validator.ValidateAsync(filter);
      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureFindResponse<UserStatInfo>(
          HttpStatusCode.BadRequest,
          validationResult.Errors.Select(e => e.ErrorMessage).ToList());
      }

      List<DbWorkTime> dbWorkTimes;
      List<DbLeaveTime> dbLeaveTimes;
      List<ProjectData> projectsData;
      List<ProjectUserData> projectUsersData = null;
      List<DepartmentData> departmentsData = null;
      Dictionary<Guid, DepartmentUserExtendedData> departmentsUsersDictionary = null;
      List<Guid> usersIds = new();
      List<Guid> managersIds;
      List<Guid> pendingIds = filter.ProjectsIds is not null && filter.ProjectsIds.Any()
        ? null
        : new();

      Guid senderId = _httpContextAccessor.HttpContext.GetUserId();

      if (filter.DepartmentsIds is not null && filter.DepartmentsIds.Any())
      {
        Task<List<DepartmentData>> departmentsDataTask = _departmentService.GetDepartmentsDataAsync(departmentsIds: filter.DepartmentsIds);

        Task<List<DepartmentUserExtendedData>> departmentsUsersTask = _departmentService.GetDepartmentsUsersAsync(
          departmentsIds: filter.DepartmentsIds,
          byEntryDate: new DateTime(filter.Year, filter.Month, 1),
          includePendingUsers: true);

        //todo - add check for more than 1 department
        if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime)
          && !(filter.DepartmentsIds?.Count == 1
            && await _departmentService.GetDepartmentUserRoleAsync(
              userId: senderId, departmentId: filter.DepartmentsIds.First()) == DepartmentUserRole.Manager))
        {
          return _responseCreator.CreateFailureFindResponse<UserStatInfo>(HttpStatusCode.Forbidden);
        }

        departmentsUsersDictionary = (await departmentsUsersTask).ToDictionary(du => du.UserId);

        foreach (DepartmentUserExtendedData departmentUser in departmentsUsersDictionary?.Values ?? Enumerable.Empty<DepartmentUserExtendedData>())
        {
          if (departmentUser.IsActive)
          {
            usersIds.Add(departmentUser.UserId);
          }
          else
          {
            pendingIds?.Add(departmentUser.UserId);
          }
        }

        departmentsData = await departmentsDataTask;
      }

      if (filter.ProjectsIds is not null && filter.ProjectsIds.Any())
      {
        projectUsersData = await _projectService.GetProjectsUsersAsync(
          projectsIds: filter.ProjectsIds,
          isActive: true,
          byEntryDate: new DateTime(filter.Year, filter.Month, 1));

        if (projectUsersData is null)
        {
          return _responseCreator.CreateFailureFindResponse<UserStatInfo>(HttpStatusCode.NotFound);
        }

        // todo - add check for multiple projects role
        if ((filter.DepartmentsIds is null || filter.DepartmentsIds.Any())
          && !(filter.ProjectsIds.Count() == 1
            && await _projectService.GetProjectUserRoleAsync(senderId, filter.ProjectsIds.FirstOrDefault()) == ProjectUserRoleType.Manager)
          && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
        {
          return _responseCreator.CreateFailureFindResponse<UserStatInfo>(HttpStatusCode.Forbidden);
        }

        usersIds = filter.DepartmentsIds is not null && filter.DepartmentsIds.Any()
          ? projectUsersData.Select(pu => pu.UserId).Distinct().Intersect(usersIds).ToList()
          : projectUsersData.Select(pu => pu.UserId).Distinct().ToList();
      }

      if (usersIds is null || !usersIds.Any())
      {
        return new();
      }

      dbWorkTimes = await _workTimeRepository.GetAsync(usersIds, null, filter.Year, filter.Month, true);
      dbLeaveTimes = await _leaveTimeRepository.GetAsync(usersIds, filter.Year, filter.Month, isActive: true);

      managersIds = dbWorkTimes.Where(wt => wt.ManagerWorkTime is not null).Select(wt => wt.ManagerWorkTime.ModifiedBy.Value)
        .Concat(dbLeaveTimes.Where(lt => lt.ManagerLeaveTime is not null).Select(lt => lt.ManagerLeaveTime.CreatedBy))
        .Distinct().ToList();

      List<string> errors = new();

      Task<List<UserData>> managersDataTask = _userService.GetUsersDataAsync(managersIds, errors);

      Task<List<ProjectData>> projectsTask = _projectService.GetProjectsDataAsync(
        projectsIds: dbWorkTimes.Select(wt => wt.ProjectId).Distinct().ToList());

      DbWorkTimeMonthLimit monthLimit = await _workTimeMonthLimitRepository.GetAsync(filter.Year, filter.Month);

      (List<UserData> usersData, int totalCount) = await _userService.GetFilteredUsersDataAsync(
        usersIds: usersIds.Concat(pendingIds ?? Enumerable.Empty<Guid>()).ToList(),
        skipCount: filter.SkipCount,
        takeCount: filter.TakeCount,
        ascendingSort: filter.AscendingSort,
        nameIncludeSubstring: filter.NameIncludeSubstring,
        isActive: null,
        errors: errors);

      Task<List<CompanyData>> companiesTask = _companyService.GetCompaniesDataAsync(
        usersData?.Select(ud => ud.Id).ToList(),
        errors);
      Task<List<PositionData>> positionsTask = _positionService.GetPositionsAsync(usersData?.Select(u => u.Id).ToList(), errors);

      List<UserInfo> usersInfos = usersData?.Select(u => _userInfoMapper.Map(u, departmentsUsersDictionary?.GetValueOrDefault(u.Id)?.IsPending)).ToList();

      List<UserInfo> managersInfos = (await managersDataTask)?.Select(ud => _userInfoMapper.Map(ud)).ToList();

      projectsData = await projectsTask;

      List<ProjectInfo> projectsInfos = projectsData?.Select(_projectInfoMapper.Map).ToList();

      List<PositionData> positionsData = await positionsTask;
      List<CompanyUserData> companiesUsersData = (await companiesTask)?.SelectMany(p => p.Users).ToList();

      return new FindResultResponse<UserStatInfo>
      {
        TotalCount = totalCount,
        Body = usersInfos?.Select(user => _userStatInfoMapper.Map(
          user: user,
          managersInfos: managersInfos,
          monthLimit: monthLimit,
          workTimes: dbWorkTimes?.Where(wt => wt.UserId == user.Id).ToList(),
          leaveTimes: dbLeaveTimes?.Where(lt => lt.UserId == user.Id).ToList(),
          projects: projectsInfos,
          position: positionsData?.FirstOrDefault(p => p.UsersIds.Contains(user.Id)),
          department: departmentsData?.FirstOrDefault(d => departmentsUsersDictionary?.GetValueOrDefault(user.Id)?.DepartmenId == d.Id),
          companyUser: companiesUsersData?.FirstOrDefault(cu => cu.UserId == user.Id))).ToList(),
        Errors = errors
      };
    }
  }
}
