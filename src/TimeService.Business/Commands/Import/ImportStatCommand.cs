using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.Import.Interfaces;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Validation.Import.Interfaces;

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

    private readonly IProjectService _projectService;
    private readonly IDepartmentService _departmentService;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly IAccessValidator _accessValidator;
    private readonly ICalendar _calendar;
    private readonly IResponseCreator _responseCreator;
    private readonly IImportStatFilterValidator _validator;

    #region private methods

    private async Task<List<DbWorkTimeMonthLimit>> GetNeededRangeOfMonthLimitsAsync(int year, int month, DateTime start, DateTime end)
    {
      List<DbWorkTimeMonthLimit> limits = await _workTimeMonthLimitRepository.GetAsync(start.Year, start.Month, end.Year, end.Month);

      int countNeededMonth = (end.Year * 12 + end.Month) - (start.Year * 12 + start.Month) + 1 - limits.Count;

      if (countNeededMonth < 1)
      {
        return limits;
      }

      int requestedYear = end.Year;
      int requestedMonth = end.Month;
      List<DbWorkTimeMonthLimit> newLimits = new();

      while (countNeededMonth > 0)
      {
        string holidays = await _calendar.GetWorkCalendarByMonthAsync(requestedMonth, requestedYear); // TODO rework

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
        if (requestedMonth == 0)
        {
          requestedMonth = 12;
          requestedYear--;
        }
      }

      await _workTimeMonthLimitRepository.CreateRangeAsync(newLimits);

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
      List<CompanyUserData> companies,
      List<DepartmentData> departments,
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

        for (int userNumber = 0; userNumber < usersInfos.Count; userNumber++)
        {
          int row = userNumber + 3;

          ws.Cell(row, 1).SetValue(userNumber + 1);
          ws.Cell(row, 2).SetValue($"{usersInfos[userNumber].FirstName} {usersInfos[userNumber].LastName}");
          ws.Cell(row, 3).SetValue(companies.FirstOrDefault(x => x.UserId == usersInfos[userNumber].Id)?.Rate);
          ws.Cell(row, 4).SetValue(thisMonthLimit.NormHours);
          ws.Cell(row, 5).SetFormulaR1C1($"=SUM({ws.Cell(row, 6).Address}:{ws.Cell(row, columnNumber - 1).Address})").
            Style.Fill.SetBackgroundColor(FirstHeaderColor);
        }

        int column = 6;
        if (filter.DepartmentId.HasValue)
        {
          DepartmentData requestedDepartment = departments.FirstOrDefault(d => d.Id == filter.DepartmentId.Value);
          
          if (requestedDepartment is not null)
          {
            CreateProjectsPartTableForDepartment(
              ws,
              column,
              projects.Where(p => requestedDepartment.ProjectsIds.Contains(p.Id)).ToList(),
              projects.Where(p => !requestedDepartment.ProjectsIds.Contains(p.Id)).ToList(),
              usersInfos,
              workTimes);
          }
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
          WriteProjectInfo(row, column++, workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[userNumber].Id && wt.ProjectId == project.Id), ws);
        }

        foreach (var project in otherProjects)
        {
          WriteProjectInfo(row, column++, workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[userNumber].Id && wt.ProjectId == project.Id), ws);
        }
      }
    }

    private void WriteProjectInfo(int row, int column, DbWorkTime wt, IXLWorksheet ws)
    {
      if (wt != null && (wt.ManagerWorkTime?.Hours != null || wt.Hours.HasValue))
      {
        ws.Cell(row, column).SetValue(wt.ManagerWorkTime?.Hours != null ? wt.ManagerWorkTime.Hours : wt.Hours);
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

        WriteProjectInfo(row, startColumn, workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[userNumber].Id && wt.ProjectId == project.Id), ws);
      }
    }

    #endregion

    #endregion

    public ImportStatCommand(
      IProjectService projectService,
      IDepartmentService departmentService,
      IUserService userService,
      ICompanyService companyService,
      IWorkTimeRepository workTimeRepository,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      IAccessValidator accessValidator,
      IResponseCreator responseCreator,
      IImportStatFilterValidator validator)
    {
      _projectService = projectService;
      _departmentService = departmentService;
      _userService = userService;
      _companyService = companyService;
      _workTimeRepository = workTimeRepository;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _accessValidator = accessValidator;
      _responseCreator = responseCreator;
      _validator = validator;
      _calendar = new IsDayOffIntegration();
    }

    public async Task<OperationResultResponse<byte[]>> ExecuteAsync(ImportStatFilter filter)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        return _responseCreator.CreateFailureResponse<byte[]>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreator.CreateFailureResponse<byte[]>(HttpStatusCode.BadRequest, errors);
      }

      List<Guid> usersIds;

      List<ProjectData> projects;
      List<ProjectUserData> projectsUsers;
      List<DepartmentData> departments = null;

      if (filter.DepartmentId.HasValue)
      {
        usersIds = (await _departmentService.GetDepartmentUsersAsync(
          filter.DepartmentId.Value,
          errors,
          byEntryDate: new DateTime(filter.Year, filter.Month, 1))).usersIds;

        projectsUsers = (await _projectService.GetProjectUsersAsync(errors, usersIds: usersIds)).projectUsersData;

        projects = await _projectService.GetProjectsDataAsync(
          errors,
          projectsIds: projectsUsers?.Select(pu => pu.ProjectId).Distinct().ToList(),
          includeUsers: false);

        departments = await _departmentService.GetDepartmentsDataAsync(errors, projectsIds: projects.Select(p => p.Id).ToList());

        if (projectsUsers == null || projects == null || departments == null)
        {
          return _responseCreator.CreateFailureResponse<byte[]>(HttpStatusCode.BadRequest);
        }
      }
      else
      {
        projects = await _projectService.GetProjectsDataAsync(
          errors,
          projectsIds: new() { filter.ProjectId.Value },
          includeUsers: true);

        usersIds = projects?.SelectMany(p => p.Users.Select(pu => pu.UserId)).OrderBy(id => id).Distinct().ToList();
      }

      List<UserData> usersInfos = await _userService.GetUsersDataAsync(usersIds, errors);

      if (usersInfos == null || projects == null)
      {
        return new()
        {
          Errors = errors
        };
      }

      List<CompanyUserData> companies = (await _companyService.GetCompaniesDataAsync(usersIds, errors))?.SelectMany(p => p.Users).ToList();
      List<DbWorkTime> workTimes = await _workTimeRepository.GetAsync(usersIds, projects.Select(p => p.Id).ToList(), filter.Year, filter.Month);
      List<DbLeaveTime> leaveTimes = await _leaveTimeRepository.GetAsync(usersIds, filter.Year, filter.Month);
      List<DbWorkTimeMonthLimit> monthsLimits;
      if (leaveTimes.Any())
      {
        monthsLimits = await GetNeededRangeOfMonthLimitsAsync(
          filter.Year,
          filter.Month,
          leaveTimes.Select(lt => lt.StartTime).Min(),
          leaveTimes.Select(lt => lt.EndTime).Max());
      }
      else
      {
        DbWorkTimeMonthLimit monthLimit = await _workTimeMonthLimitRepository.GetAsync(filter.Year, filter.Month);

        if (monthLimit == null)
        {
          errors.Add("Cannot get month limit.");

          return _responseCreator.CreateFailureResponse<byte[]>(HttpStatusCode.BadRequest, errors);
        }
        else
        {
          monthsLimits = new() { monthLimit };
        }
      }

      return new()
      {
        Body = CreateTable(filter, usersInfos, projects, companies, departments, workTimes, leaveTimes, monthsLimits)
      };
    }
  }
}
