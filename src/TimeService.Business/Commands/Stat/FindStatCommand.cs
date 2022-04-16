using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
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
    private readonly IDepartmentService _departmentService;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IProjectService _projectService;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IStatInfoMapper _statInfoMapper;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly ILogger<FindStatCommand> _logger;
    private readonly IResponseCreator _responseCreator;
    private readonly IFindStatFilterValidator _validator;

    public FindStatCommand(
      IDepartmentService departmentService,
      IUserService userService,
      ICompanyService companyService,
      IProjectService projectService,
      IUserInfoMapper userInfoMapper,
      IProjectInfoMapper projectInfoMapper,
      IStatInfoMapper statInfoMapper,
      IWorkTimeRepository workTimeRepository,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      ILogger<FindStatCommand> logger,
      IResponseCreator responseCreator,
      IFindStatFilterValidator validator)
    {
      _departmentService = departmentService;
      _userService = userService;
      _companyService = companyService;
      _projectService = projectService;
      _userInfoMapper = userInfoMapper;
      _projectInfoMapper = projectInfoMapper;
      _statInfoMapper = statInfoMapper;
      _workTimeRepository = workTimeRepository;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _logger = logger;
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
      List<ProjectData> projectsData;
      List<ProjectUserData> projectUsersData = new();
      List<Guid> usersIds = new();

      if (filter.ProjectId is not null)
      {
        (projectUsersData, totalCount) = await _projectService.GetProjectUsersAsync(filter.ProjectId.Value, filter.SkipCount, filter.TakeCount, errors);

        usersIds = projectUsersData.Select(pu => pu.UserId).ToList();
      }
      else if (filter.DepartmentsIds?.Count > 1)
      {
        List<DepartmentFilteredData> departmentsData = await _departmentService.GetDepartmentFilteredDataAsync(filter.DepartmentsIds, errors);

        departmentsData.ForEach(x => usersIds.AddRange(x.UsersIds));
      }
      else if (filter.DepartmentsIds?.Count == 1)
      {
        (usersIds, totalCount) = await _departmentService.GetDepartmentUsersAsync(
          filter.DepartmentsIds.FirstOrDefault(),
          filter.SkipCount,
          filter.TakeCount,
          errors);
      }

      dbWorkTimes = await _workTimeRepository.GetAsync(usersIds, null, filter.Year, filter.Month, true);
      dbLeaveTimes = await _leaveTimeRepository.GetAsync(usersIds, filter.Year, filter.Month);

      projectsData = await _projectService.GetProjectsDataAsync(dbWorkTimes.Select(wt => wt.ProjectId).Distinct().ToList(), errors);

      DbWorkTimeMonthLimit monthLimit = filter.Month.HasValue
        ? await _workTimeMonthLimitRepository.GetAsync(filter.Year, filter.Month.Value)
        : null;

      Task<List<UserData>> usersTask = _userService.GetUsersDataAsync(usersIds, errors);
      Task<List<CompanyData>> companiesTask = _companyService.GetCompaniesDataAsync(usersIds, errors);

      await Task.WhenAll(usersTask, companiesTask);

      List<CompanyUserData> companies = (await companiesTask)?.SelectMany(p => p.Users).ToList();

      List<UserInfo> usersInfos = (await usersTask)
        ?.Select(u => _userInfoMapper.Map(u, companies?.FirstOrDefault(p => p.UserId == u.Id))).ToList();

      List<ProjectInfo> projectInfos = projectsData?.Select(_projectInfoMapper.Map).ToList();

      return new FindResultResponse<StatInfo>
      {
        TotalCount = totalCount,
        Body = usersIds.Select(
          id => _statInfoMapper.Map(
            id,
            usersInfos?.FirstOrDefault(u => u.Id == id),
            projectUsersData?.FirstOrDefault(pu => pu.UserId == id),
            monthLimit,
            dbWorkTimes?.Where(wt => wt.UserId == id).ToList(),
            projectInfos,
            dbLeaveTimes.Where(lt => lt.UserId == id).ToList()))
          .ToList(),
        Errors = errors,
        Status = errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess
      };
    }
  }
}
