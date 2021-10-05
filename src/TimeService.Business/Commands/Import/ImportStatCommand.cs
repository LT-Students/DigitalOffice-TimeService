using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
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
using LT.DigitalOffice.TimeService.Business.Commands.Import.Interfaces;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LT.DigitalOffice.TimeService.Business.Commands.Import
{
  public class ImportStatCommand : IImportStatCommand
  {
    private XLColor FirstHeaderColor => XLColor.LightApricot;
    private XLColor SecondHeaderColor => XLColor.LightSkyBlue;
    private XLColor MainProjectColor => XLColor.LimeGreen;
    private XLColor OtherProjectColor => XLColor.LightGray;
    private XLColor LeaveTypesColor => XLColor.CarrotOrange;
    private XLColor TimesColor => XLColor.LightGreen;

    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetProjectsUsersRequest> _rcGetProjectsUsers;
    private readonly IRequestClient<IGetDepartmentUsersRequest> _rcGetDepartmentUsers;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly ILogger<ImportStatCommand> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly ICalendar _calendar;
    private readonly IConnectionMultiplexer _cache;

    #region private methods

    private List<ProjectData> GetProjects(List<Guid> projectIds, bool includeUsers, List<string> errors)
    {
      string messageError = "Cannot get projects info. Please, try again later.";
      const string logError = "Cannot get projects info.";

      if (projectIds == null || !projectIds.Any())
      {
        return null;
      }

      try
      {
        IOperationResult<IGetProjectsResponse> result = _rcGetProjects.GetResponse<IOperationResult<IGetProjectsResponse>>(
            IGetProjectsRequest.CreateObj(
              projectsIds: projectIds,
              includeUsers: includeUsers)).Result.Message;

        if (result.IsSuccess)
        {
          return result.Body.Projects;
        }

        _logger.LogWarning(logError + "Errors: {errors}.", string.Join("\n", result.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logError);
      }

      errors.Add(messageError);
      return null;
    }

    private List<ProjectUserData> GetProjectsUsers(List<Guid> usersIds, List<string> errors)
    {
      string messageError = "Cannot get projects users info. Please, try again later.";
      const string logError = "Cannot get projects users info.";

      if (usersIds == null || !usersIds.Any())
      {
        return null;
      }

      try
      {
        IOperationResult<IGetProjectsUsersResponse> result = _rcGetProjectsUsers.GetResponse<IOperationResult<IGetProjectsUsersResponse>>(
          IGetProjectsUsersRequest.CreateObj(
            usersIds: usersIds)).Result.Message;

        if (result.IsSuccess)
        {
          return result.Body.Users;
        }

        _logger.LogWarning(logError + "Errors: {errors}.", string.Join("\n", result.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logError);
      }

      errors.Add(messageError);
      return null;
    }

    private List<Guid> FindDepartmentUsers(Guid departmentId, DateTime filterbyEntryDate, List<string> errors)
    {
      string messageError = "Cannot get department users info. Please, try again later.";
      const string logError = "Cannot get users of department with id: '{id}'.";

      try
      {
        IOperationResult<IGetDepartmentUsersResponse> result = _rcGetDepartmentUsers.GetResponse<IOperationResult<IGetDepartmentUsersResponse>>(
            IGetDepartmentUsersRequest.CreateObj(departmentId, ByEntryDate: filterbyEntryDate)).Result.Message;

        if (result.IsSuccess)
        {
          return result.Body.UserIds;
        }

        _logger.LogWarning(logError + "Errors: {errors}.", departmentId, string.Join("\n", result.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logError, departmentId);
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
        _logger.LogInformation("UsersDatas were taken from the cache.");

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
        Response<IOperationResult<IGetUsersDataResponse>> response = await _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
          IGetUsersDataRequest.CreateObj(userIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("UsersDatas were taken from the service.");

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

    private List<DbWorkTimeMonthLimit> GetNeededRangeOfMonthLimits(int year, int month, DateTime start, DateTime end)
    {
      List<DbWorkTimeMonthLimit> limits = _workTimeMonthLimitRepository.GetRange(start.Year, start.Month, end.Year, end.Month);

      int countNeededMonth = (end.Year * 12 + end.Month) - (start.Year * 12 + start.Month) + 1 - limits.Count;

      if (countNeededMonth < 1)
      {
        return limits;
      }

      int requestedYear = end.Year;
      int requestedMonth = end.Month;
      List<DbWorkTimeMonthLimit> newLimits = new();

      while(countNeededMonth > 0)
      {
        string holidays = _calendar.GetWorkCalendarByMonth(requestedMonth, requestedYear);

        if (holidays == null)
        {
          throw new InvalidOperationException("Cannot get holidays.");
        }

        newLimits.Add(
          new DbWorkTimeMonthLimit
          {
            Id = Guid.NewGuid(),
            Year = requestedYear,
            Month = requestedMonth,
            Holidays = holidays,
            NormHours = holidays.ToCharArray().Count(h => h == '0') * 8
          });

        countNeededMonth--;
        requestedMonth--;
        if(requestedMonth == 0)
        {
          requestedMonth = 12;
          requestedYear--;
        }
      }

      _workTimeMonthLimitRepository.AddRange(newLimits);

      limits.AddRange(newLimits);

      return limits;
    }

    #region Create table

    private float GetLeaveTimeOurs(List<DbWorkTimeMonthLimit> monthsLimits, DbLeaveTime leaveTime, int year, int month)
    {
      // monthsLimits should be sort by time

      int requestedMonthCount = year * 12 + month;
      int startCountMonths = leaveTime.StartTime.Year * 12 + leaveTime.StartTime.Month;
      int endCountMonths = leaveTime.EndTime.Year * 12 + leaveTime.EndTime.Month;

      List<DbWorkTimeMonthLimit> monthRange = monthsLimits
        .Where(ml => ml.Year * 12 + ml.Month >= startCountMonths && ml.Year * 12 + ml.Month <= endCountMonths)
        .ToList();

      if (monthsLimits.Count == 1)
      {
        return (float)leaveTime.Minutes / 60;
      }

      float countWorkingHours = monthRange.Select(ml => ml.NormHours).Sum();

      DbWorkTimeMonthLimit first = monthRange.First();

      float extraHoursInFirstMonth = (float)first.Holidays.Substring(0, leaveTime.StartTime.Day).Count(d => d == '0')
        / first.Holidays.Count(d => d == '0') * first.NormHours;

      DbWorkTimeMonthLimit last = monthRange.Last();

      float extraHoursInLastMonth = (1 - (float)last.Holidays.Substring(0, leaveTime.EndTime.Day).Count(d => d == '0')
        / last.Holidays.Count(d => d == '0')) * last.NormHours;

      countWorkingHours -= extraHoursInFirstMonth + extraHoursInLastMonth;

      DbWorkTimeMonthLimit thisMonth = monthRange.First(ml => ml.Year == year && ml.Month == month);
      float countWorkingHoursOfThisMonth = thisMonth.NormHours;

      if (thisMonth == first)
      {
        countWorkingHoursOfThisMonth -= extraHoursInFirstMonth;
      }

      if (thisMonth == last)
      {
        countWorkingHoursOfThisMonth -= extraHoursInLastMonth;
      }

      return countWorkingHoursOfThisMonth / countWorkingHours * leaveTime.Minutes / 60;
    }

    private void AddHeaderCell(IXLWorksheet ws, int column, string value, XLColor color)
    {
      IXLCell cell = ws.Cell(1, column);
      cell.SetValue(value);
      if (value.Length > 3)
      {
        cell.Style.Alignment.TextRotation = 90;
      }
      cell.Style
        .Font.SetBold()
        .Fill.SetBackgroundColor(color);
    }

    private byte[] CreateTable(
      ImportStatFilter filter,
      List<UserData> usersInfos,
      List<ProjectData> projects,
      List<DbWorkTime> workTimes,
      List<DbLeaveTime> leaveTimes,
      List<DbWorkTimeMonthLimit> monthsLimits)
    {
      monthsLimits = monthsLimits.OrderBy(ml => ml.Year * 12 + ml.Month).ToList();
      DbWorkTimeMonthLimit thisMonthLimit = monthsLimits.First(ml => ml.Year == filter.Year && ml.Month == filter.Month);
      MemoryStream ms = new MemoryStream();

      List<string> headers = new()
      {
        "№",
        "Initials",
        "Rate",
        "Standard working hours",
        "Total"
      };

      using (var workbook = new XLWorkbook())
      {
        IXLWorksheet ws = workbook.Worksheets.Add("Hours");

        IXLRange range = ws.Range(1, 1, 1, headers.Count);

        int columnNumber = 1;
        foreach (var columnName in headers)
        {
          AddHeaderCell(ws, columnNumber, columnName, FirstHeaderColor);
          columnNumber++;
        }

        columnNumber += projects.Count;

        int leaveTypeCount = 0;
        foreach (var leaveName in Enum.GetValues(typeof(LeaveType)))
        {
          AddHeaderCell(ws, columnNumber, leaveName.ToString(), LeaveTypesColor);
          columnNumber++;
          leaveTypeCount++;
        }

        ws.Cell(2, 2).Value = "Total";
        ws.Cell(2, 2).Style.Font.SetBold();
        ws.Range(2, 1, 2, columnNumber - 1).Style.Fill.SetBackgroundColor(SecondHeaderColor);

        for (int i = 5; i < columnNumber; i++)
        {
          ws.Cell(2, i).SetFormulaR1C1($"=SUM({ws.Cell(3, i).Address}:{ws.Cell(2 + usersInfos.Count(), i).Address})");
        }

        for (int number = 0; number < usersInfos.Count; number++)
        {
          int row = number + 3;

          ws.Cell(row, 1).SetValue(number + 1);
          ws.Cell(row, 2).SetValue($"{usersInfos[number].FirstName} {usersInfos[number].LastName}");
          ws.Cell(row, 3).SetValue(usersInfos[number].Rate);
          ws.Cell(row, 4).SetValue(thisMonthLimit.NormHours);
          ws.Cell(row, 5).SetFormulaR1C1($"=SUM({ws.Cell(row, 6).Address}:{ws.Cell(row, columnNumber - 1).Address})").
            Style.Fill.SetBackgroundColor(FirstHeaderColor);
        }

        int column = 6;
        if (filter.DepartmentId.HasValue)
        {
          CreateProjectsPartTableForDepartment(
            ws,
            column,
            projects.Where(p => p.DepartmentId == filter.DepartmentId.Value).ToList(),
            projects.Where(p => p.DepartmentId != filter.DepartmentId.Value).ToList(),
            usersInfos,
            workTimes);
        }
        else
        {
          CreateProjectPartTable(
            ws,
            column,
            projects[0],
            usersInfos,
            workTimes);
        }

        column += projects.Count;

        if (leaveTimes != null && leaveTimes.Any())
        {
          for (int number = 0; number < usersInfos.Count; number++)
          {
            int row = number + 3;

            List<DbLeaveTime> usersLeaveTimes = leaveTimes.Where(lt => lt.UserId == usersInfos[number].Id).ToList();

            for (int i = 0; i < leaveTypeCount; i++)
            {
              ws.Cell(row, column + i).SetValue(
                  usersLeaveTimes
                  .Where(lt => lt.LeaveType == i)
                  .Select(lt => GetLeaveTimeOurs(monthsLimits, lt, filter.Year, filter.Month))
                  .Sum());
            }
          }
        }

        ws.Range(1, 1, 2 + usersInfos.Count, columnNumber - 1).Style
          .Border.SetBottomBorder(XLBorderStyleValues.Thin)
          .Border.SetLeftBorder(XLBorderStyleValues.Thin)
          .Border.SetRightBorder(XLBorderStyleValues.Thin)
          .Border.SetTopBorder(XLBorderStyleValues.Thin);
        ws.Range(3, 6, 2 + usersInfos.Count, columnNumber - 1).Style.Fill.SetBackgroundColor(TimesColor);

        workbook.SaveAs(ms);
      }

      return ms.ToArray();
    }

    private void CreateProjectsPartTableForDepartment(
      IXLWorksheet ws,
      int startColumn,
      List<ProjectData> departmentProjects,
      List<ProjectData> otherProjects,
      List<UserData> usersInfos,
      List<DbWorkTime> workTimes)
    {
      int column = startColumn;

      foreach (var project in departmentProjects)
      {
        AddHeaderCell(ws, column, project.Name, MainProjectColor);
        column++;
      }

      foreach (var project in otherProjects)
      {
        AddHeaderCell(ws, column, project.Name, OtherProjectColor);
        column++;
      }

      for (int userNumber = 0; userNumber < usersInfos.Count; userNumber++)
      {
        int row = userNumber + 3;
        column = startColumn;

        foreach (var project in departmentProjects)
        {
          var wt = workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[userNumber].Id && wt.ProjectId == project.Id);

          if (wt != null && (wt.ManagerHours.HasValue || wt.UserHours.HasValue))
          {
            ws.Cell(row, column).SetValue(wt.ManagerHours.HasValue ? wt.ManagerHours : wt.UserHours);
          }

          column++;
        }

        foreach (var project in otherProjects)
        {
          var wt = workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[userNumber].Id && wt.ProjectId == project.Id);

          if (wt != null && (wt.ManagerHours.HasValue || wt.UserHours.HasValue))
          {
            ws.Cell(row, column).SetValue(wt.ManagerHours.HasValue ? wt.ManagerHours : wt.UserHours);
          }

          column++;
        }
      }
    }

    private void CreateProjectPartTable(
      IXLWorksheet ws,
      int startColumn,
      ProjectData project,
      List<UserData> usersInfos,
      List<DbWorkTime> workTimes)
    {
      AddHeaderCell(ws, startColumn, project.Name, MainProjectColor);

      for (int userNumber = 0; userNumber < usersInfos.Count; userNumber++)
      {
        int row = userNumber + 3;

        var wt = workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[userNumber].Id && wt.ProjectId == project.Id);

        if (wt != null && (wt.ManagerHours.HasValue || wt.UserHours.HasValue))
        {
          ws.Cell(row, startColumn).SetValue(wt.ManagerHours.HasValue ? wt.ManagerHours : wt.UserHours);
        }
      }
    }
    #endregion

    #endregion

    public ImportStatCommand(
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetProjectsUsersRequest> rcGetProjectsUsers,
      IRequestClient<IGetDepartmentUsersRequest> rcGetDepartmentUsers,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      IWorkTimeRepository workTimeRepository,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      ILogger<ImportStatCommand> logger,
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      IConnectionMultiplexer cache)
    {
      _rcGetProjects = rcGetProjects;
      _rcGetProjectsUsers = rcGetProjectsUsers;
      _rcGetDepartmentUsers = rcGetDepartmentUsers;
      _rcGetUsers = rcGetUsers;
      _workTimeRepository = workTimeRepository;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _logger = logger;
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _cache = cache;
      _calendar = new IsDayOffIntegration();
    }

    public async Task<OperationResultResponse<byte[]>> Execute(ImportStatFilter filter)
    {
      if (!_accessValidator.IsAdmin())
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        return new OperationResultResponse<byte[]>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new() { "Not enough rights." }
        };
      }

      if (!filter.DepartmentId.HasValue && !filter.ProjectId.HasValue
        || filter.DepartmentId.HasValue && filter.ProjectId.HasValue)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new OperationResultResponse<byte[]>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new List<string> { "The request must contain either the id of the department or the project." }
        };
      }

      List<string> errors = new();
      List<Guid> usersIds = null;

      List<ProjectData> projects;
      List<ProjectUserData> projectsUsers;

      if (filter.DepartmentId.HasValue)
      {
        usersIds = FindDepartmentUsers(filter.DepartmentId.Value, new DateTime(filter.Year, filter.Month, 1), errors);

        projectsUsers = GetProjectsUsers(usersIds, errors);

        projects = GetProjects(projectsUsers?.Select(pu => pu.ProjectId).Distinct().ToList(), false, errors);
      }
      else
      {
        projects = GetProjects(new() { filter.ProjectId.Value }, true, errors);

        usersIds = projects?.SelectMany(p => p.Users.Select(pu => pu.UserId)).OrderBy(id => id).Distinct().ToList();
      }

      List<UserData> usersInfos = await GetUsersData(usersIds, errors);

      if (usersInfos == null || projects == null)
      {
        return new()
        {
          Status = OperationResultStatusType.Failed,
          Errors = errors
        };
      }

      List<DbWorkTime> workTimes = _workTimeRepository.Find(usersIds, projects.Select(p => p.Id).ToList(), filter.Year, filter.Month);
      List<DbLeaveTime> leaveTimes = _leaveTimeRepository.Find(usersIds, filter.Year, filter.Month);
      List<DbWorkTimeMonthLimit> monthsLimits;
      if (leaveTimes.Any())
      {
        monthsLimits = GetNeededRangeOfMonthLimits(
          filter.Year,
          filter.Month,
          leaveTimes.Select(lt => lt.StartTime).Min(),
          leaveTimes.Select(lt => lt.EndTime).Max());
      }
      else
      {
        DbWorkTimeMonthLimit monthLimit = _workTimeMonthLimitRepository.Get(filter.Year, filter.Month);

        if (monthLimit == null)
        {
          errors.Add("Cannot get month limit.");

          return new()
          {
            Status = OperationResultStatusType.Failed,
            Errors = errors
          };
        }
        else
        {
          monthsLimits = new() { monthLimit };
        }
      }

      return new()
      {
        Status = OperationResultStatusType.FullSuccess,
        Body = CreateTable(filter, usersInfos, projects, workTimes, leaveTimes, monthsLimits)
      };
    }
  }
}
