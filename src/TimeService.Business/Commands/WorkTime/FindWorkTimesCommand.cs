using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
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
    private readonly IProjectService _projectService;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IWorkTimeResponseMapper _workTimeResponseMapper;
    private readonly IProjectInfoMapper _projectInfoMapper;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IResponseCreator _responsCreator;
    private readonly ILogger<FindWorkTimesCommand> _logger;

    #region private methods

    private async Task<List<ProjectData>> GetProjects(List<Guid> projectsIds, Guid? userId, List<string> errors)
    {
      if (projectsIds is null || !projectsIds.Any())
      {
        return null;
      }

      return await _projectService.GetProjectsDataAsync(errors, projectsIds: projectsIds, userId: userId, includeUsers: true);
    }

    private async Task<List<UserData>> GetUsersData(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return null;
      }

      return await _userService.GetUsersDataAsync(usersIds: usersIds, errors: errors);
    }

    private async Task<List<CompanyData>> GetCompaniesAsync(
      List<Guid> usersIds,
      List<string> errors)
    {
      if (usersIds is null || !usersIds.Any())
      {
        return null;
      }

      return await _companyService.GetCompaniesDataAsync(usersIds: usersIds, errors: errors);
    }

    #endregion

    public FindWorkTimesCommand(
      IBaseFindFilterValidator validator,
      IWorkTimeResponseMapper workTimeResponseMapper,
      IWorkTimeRepository repository,
      IWorkTimeMonthLimitRepository monthLimitRepository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IProjectService projectService,
      IUserService userService,
      ICompanyService companyService,
      ILogger<FindWorkTimesCommand> logger,
      IProjectInfoMapper projectInfoMapper,
      IUserInfoMapper userInfoMapper,
      IGlobalCacheRepository globalCache,
      IResponseCreator responsCreator)
    {
      _validator = validator;
      _workTimeResponseMapper = workTimeResponseMapper;
      _workTimeRepository = repository;
      _monthLimitRepository = monthLimitRepository;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _projectService = projectService;
      _userService = userService;
      _companyService = companyService;
      _logger = logger;
      _projectInfoMapper = projectInfoMapper;
      _userInfoMapper = userInfoMapper;
      _globalCache = globalCache;
      _responsCreator = responsCreator;
    }

    public async Task<FindResultResponse<WorkTimeResponse>> ExecuteAsync(FindWorkTimesFilter filter)
    {
      var isActhor = filter.UserId.HasValue && filter.UserId == _httpContextAccessor.HttpContext.GetUserId();

      if (!isActhor && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        return _responsCreator.CreateFailureFindResponse<WorkTimeResponse>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(filter, out List<string> errors))
      {
        return _responsCreator.CreateFailureFindResponse<WorkTimeResponse>(HttpStatusCode.BadRequest, errors);
      }

      (List<DbWorkTime> dbWorkTimes, int totalCount) = await _workTimeRepository.FindAsync(filter);

      List<Guid> usersIds = dbWorkTimes.Select(wt => wt.UserId).Distinct().ToList();
      usersIds.AddRange(dbWorkTimes.Where(wt => wt.ManagerWorkTime != null).Select(wt => wt.ManagerWorkTime.ModifiedBy.Value).ToList());

      Task<List<ProjectData>> projectsTask = GetProjects(dbWorkTimes.Select(wt => wt.ProjectId).Distinct().ToList(), filter.UserId, errors);
      Task<List<UserData>> usersTask = GetUsersData(usersIds, errors);
      Task<List<CompanyData>> companiesTask = GetCompaniesAsync(usersIds, errors);
      Task<(List<DbWorkTimeMonthLimit>, int)> limitTask = _monthLimitRepository.FindAsync(
        new()
        {
          Month = filter.Month,
          Year = filter.Year
        });

      await Task.WhenAll(projectsTask, usersTask, companiesTask, limitTask);

      List<CompanyData> companies = await companiesTask;
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
                companies?.FirstOrDefault(p => p.Users.Any(u => u.UserId == wt.UserId))?.Users.First(u => u.UserId == wt.UserId)),
              _userInfoMapper.Map(
                users?.FirstOrDefault(u => u.Id == wt.ManagerWorkTime?.ModifiedBy),
                null),
              project?.Users.FirstOrDefault(pu => pu.UserId == wt.UserId),
              _projectInfoMapper.Map(project));
          }).ToList(),
        Errors = errors
      };
    }
  }
}
