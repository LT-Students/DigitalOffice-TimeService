﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Image;
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
    private readonly IImageService _imageService;
    private readonly IPositionService _positionService;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IStatInfoMapper _statInfoMapper;
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
      IImageService imageService,
      IPositionService positionService,
      IUserInfoMapper userInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IStatInfoMapper statInfoMapper,
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
      _imageService = imageService;
      _positionService = positionService;
      _userInfoMapper = userInfoMapper;
      _projectInfoMapper = projectInfoMapper;
      _statInfoMapper = statInfoMapper;
      _workTimeRepository = workTimeRepository;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _responseCreator = responseCreator;
      _validator = validator;
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
    }

    public async Task<FindResultResponse<StatInfo>> ExecuteAsync(FindStatFilter filter)
    {
      if (!_validator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreator.CreateFailureFindResponse<StatInfo>(HttpStatusCode.BadRequest, errors);
      }

      List<DbWorkTime> dbWorkTimes;
      List<DbLeaveTime> dbLeaveTimes;
      List<ProjectData> projectsData;
      List<ProjectUserData> projectUsersData = default;
      List<DepartmentData> departmentsData = default;
      List<Guid> usersIds = new();

      Guid senderId = _httpContextAccessor.HttpContext.GetUserId();

      if (filter.ProjectId is not null)
      {
        (projectUsersData, _) = await _projectService.GetProjectUsersAsync(errors, new List<Guid> { filter.ProjectId.Value });

        if (projectUsersData is null)
        {
          return _responseCreator.CreateFailureFindResponse<StatInfo>(HttpStatusCode.NotFound);
        }

        if (projectUsersData.FirstOrDefault(x => x.UserId == senderId)?.ProjectUserRole != ProjectUserRoleType.Manager
          && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
        {
          return _responseCreator.CreateFailureFindResponse<StatInfo>(HttpStatusCode.Forbidden);
        }

        usersIds = projectUsersData.Select(pu => pu.UserId).ToList();
      }
      else
      {
        departmentsData = await _departmentService.GetDepartmentsDataAsync(errors, departmentsIds: filter.DepartmentsIds);

        if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime)
          && !(filter.DepartmentsIds?.Count == 1
            && departmentsData?.FirstOrDefault()?.Users.FirstOrDefault(user => user.UserId == senderId)?.Role == DepartmentUserRole.Manager))
        {
          return _responseCreator.CreateFailureFindResponse<StatInfo>(HttpStatusCode.Forbidden);
        }

        departmentsData?.ForEach(x => usersIds.AddRange(x.Users.Select(user => user.UserId)));
      }

      dbWorkTimes = await _workTimeRepository.GetAsync(usersIds, null, filter.Year, filter.Month, true);
      dbLeaveTimes = await _leaveTimeRepository.GetAsync(usersIds, filter.Year, filter.Month);

      projectsData = await _projectService.GetProjectsDataAsync(
        errors,
        projectsIds: dbWorkTimes.Select(wt => wt.ProjectId).Distinct().ToList(),
        includeUsers: false);

      DbWorkTimeMonthLimit monthLimit = filter.Month.HasValue
        ? await _workTimeMonthLimitRepository.GetAsync(filter.Year, filter.Month.Value)
        : null;

      (List<UserData> usersData, int totalCount) = await _userService.GetFilteredUsersDataAsync(
        usersIds,
        filter.SkipCount,
        filter.TakeCount,
        filter.AscendingSort,
        errors);

      Task<List<CompanyData>> companiesTask = _companyService.GetCompaniesDataAsync(
        usersData.Select(ud => ud.Id).ToList(),
        errors);
      Task<List<ImageData>> imagesTask = _imageService.GetImagesAsync(usersIds, errors);
      Task<List<PositionData>> positionsTask = _positionService.GetPositionsAsync(usersIds, errors);

      await Task.WhenAll(
        companiesTask,
        imagesTask,
        positionsTask);

      List<CompanyUserData> companies = (await companiesTask)?.SelectMany(p => p.Users).ToList();

      List<UserInfo> usersInfos = usersData
        .Select(u => _userInfoMapper.Map(
          u,
          imagesTask.Result.FirstOrDefault(i => i.ParentId == u.Id))).ToList();

      List<ProjectInfo> projectInfos = projectsData?.Select(_projectInfoMapper.Map).ToList();

      return new FindResultResponse<StatInfo>
      {
        TotalCount = totalCount,
        Body = _statInfoMapper.Map(
          departmentsData,
          usersIds,
          usersInfos,
          projectUsersData,
          monthLimit,
          dbWorkTimes,
          projectInfos,
          dbLeaveTimes,
          positionsTask.Result,
          companies),
        Errors = errors
      };
    }
  }
}
