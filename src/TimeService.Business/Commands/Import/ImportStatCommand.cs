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
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Validation.Import.Interfaces;

namespace LT.DigitalOffice.TimeService.Business.Commands.Import
{
  public class ImportStatCommand : IImportStatCommand
  {
    private XLColor FirstHeaderColor => XLColor.LightPastelPurple;
    private XLColor SecondHeaderColor => XLColor.LightSkyBlue;
    private XLColor MainProjectColor => XLColor.LightGreen;
    private XLColor OtherProjectColor => XLColor.LightYellow;
    private XLColor VacantionColor => XLColor.PastelOrange;
    private XLColor LeaveTypesColor => XLColor.LavenderBlue;
    private XLColor TimesColor => XLColor.LightGreen;

    private readonly Dictionary<LeaveType, string> _leaveTypesNamesRu = 
      new()
      {
        { LeaveType.Vacation, "Оплачиваемый отпуск" },
        { LeaveType.SickLeave, "Больничный" },
        { LeaveType.Training, "Обучение" },
        { LeaveType.Idle, "За свой счет" }
      };

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
    private readonly IUserImportStatInfoMapper _mapper;

    #region private methods

    private async Task<List<DbWorkTimeMonthLimit>> GetNeededRangeOfMonthLimitsAsync(DateTime start, DateTime end, List<string> errors = null)
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

        if (holidays is null)
        {
          errors?.Add("Cannot get holidays.");

          return null;
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

    private float GetLeaveTimeHours(List<DbWorkTimeMonthLimit> monthsLimits, DbLeaveTime leaveTime, int year, int month)
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

      float extraHoursInFirstMonth = (float)first.Holidays.Substring(0, leaveTime.StartTime.Day - 1).Count(d => d == '0')
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
      List<UserImportStatInfo> sortedUsers,
      List<ProjectData> mainProjects,
      List<ProjectData> otherProjects,
      DepartmentData department,
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
        "ФИО",
        "Ставка",
        "Норма часов",
        "Итого"
      };

      List<string> lastHeaders = new()
      {
        "Тип договора"
      };

      using (var workbook = new XLWorkbook(XLEventTracking.Disabled))
      {
        IXLWorksheet ws = workbook.Worksheets.Add("Hours");

        int columnNumber = 1;

        foreach (var columnName in headers)
        {
          AddHeaderCell(ws, columnNumber, columnName, FirstHeaderColor);
          columnNumber++;
        }

        columnNumber += mainProjects.Count;

        int leaveTypesCount = 0;
        foreach (var leaveType in Enum.GetValues<LeaveType>())
        {
          if (leaveType == LeaveType.Vacation)
          {
            AddHeaderCell(ws, columnNumber, _leaveTypesNamesRu[leaveType], VacantionColor);
          }
          else
          {
            AddHeaderCell(ws, columnNumber, _leaveTypesNamesRu[leaveType], LeaveTypesColor);
          }

          columnNumber++;
          leaveTypesCount++;
        }

        int columnsCount = headers.Count + leaveTypesCount + mainProjects.Count + otherProjects.Count + lastHeaders.Count;
        columnNumber += otherProjects.Count;

        foreach (string header in lastHeaders)
        {
          AddHeaderCell(ws, columnNumber, header, FirstHeaderColor);

          columnNumber++;
        }

        ws.Cell(2, 2).Value = "Итого";
        ws.Cell(2, 2).Style.Font.SetBold();
        ws.Range(2, 1, 2, columnsCount).Style.Fill.SetBackgroundColor(SecondHeaderColor);

        for (int currentColumn = headers.Count; currentColumn <= columnsCount - lastHeaders.Count; currentColumn++)
        {
          ws.Cell(2, currentColumn).SetFormulaR1C1($"=SUM({ws.Cell(3, currentColumn).Address}:{ws.Cell(2 + sortedUsers.Count(), currentColumn).Address})");
        } 

        for (int userNumber = 0; userNumber < sortedUsers.Count(); userNumber++)
        {
          int row = userNumber + 3;
          double? rate = sortedUsers[userNumber].CompanyUserData?.Rate;

          ws.Cell(row, 1).SetValue(userNumber + 1);
          ws.Cell(row, 2).SetValue($"{sortedUsers[userNumber].UserData.LastName} {sortedUsers[userNumber].UserData.FirstName}");
          ws.Cell(row, 3).SetValue(rate);
          ws.Cell(row, 4).SetValue(thisMonthLimit.NormHours * rate);
          ws.Cell(row, 5).SetFormulaR1C1($"=SUM({ws.Cell(row, 6).Address}:{ws.Cell(row, columnsCount - lastHeaders.Count).Address})")
            .Style.Fill.SetBackgroundColor(FirstHeaderColor);
          ws.Cell(row, columnNumber - 1).SetValue(sortedUsers[userNumber].CompanyUserData?.ContractSubject?.Name)
            .Style.Fill.SetBackgroundColor(FirstHeaderColor);
        }

        ws.Column(2).AdjustToContents();
        ws.Columns(columnsCount - lastHeaders.Count + 1, columnsCount).AdjustToContents();

        if (filter.DepartmentId.HasValue)
        {
          CreateProjectsPartTable(
            ws: ws,
            mainProjectsStartColumn: headers.Count + 1,
            otherProjectsStartColumn: headers.Count + mainProjects.Count + leaveTypesCount + 1,
            mainProjects: mainProjects,
            otherProjects: otherProjects,
            sortedUsers: sortedUsers.Select(x => x.UserData).ToList(),
            workTimes: workTimes);
        }
        else
        {
          /*CreateProjectPartTable(
            ws,
            headers.Count + 1,
            mainProjects[0],
            sortedUsers.Select(x => x.UserData).ToList(),
            workTimes);*/
          CreateProjectsPartTable(
            ws: ws,
            mainProjectsStartColumn: headers.Count + 1,
            otherProjectsStartColumn: headers.Count + mainProjects.Count + leaveTypesCount + 1,
            mainProjects: mainProjects,
            otherProjects: otherProjects,
            sortedUsers: sortedUsers.Select(x => x.UserData).ToList(),
            workTimes: workTimes);
        }

        columnNumber = headers.Count + mainProjects.Count + 1;

        if (leaveTimes is not null && leaveTimes.Any())
        {
          for (int number = 0; number < sortedUsers.Count; number++)
          {
            int row = number + 3;

            List<DbLeaveTime> usersLeaveTimes = leaveTimes.Where(lt => lt.UserId == sortedUsers[number].UserData.Id).ToList();

            for (int i = 0; i < leaveTypesCount; i++)
            {
              float leaveTimeHours = usersLeaveTimes.Where(lt => lt.LeaveType == i)
                .Select(lt => GetLeaveTimeHours(monthsLimits, lt, filter.Year, filter.Month))
                .Sum();

              if (leaveTimeHours != 0)
              {
                ws.Cell(row, columnNumber + i).SetValue(leaveTimeHours);
              }
            }
          }
        }

        ws.Range(1, 1, 2 + sortedUsers.Count, columnsCount).Style
          .Border.SetBottomBorder(XLBorderStyleValues.Thin)
          .Border.SetLeftBorder(XLBorderStyleValues.Thin)
          .Border.SetRightBorder(XLBorderStyleValues.Thin)
          .Border.SetTopBorder(XLBorderStyleValues.Thin);
        ws.Range(3, 6, 2 + sortedUsers.Count, columnsCount - lastHeaders.Count).Style.Fill.SetBackgroundColor(TimesColor);

        workbook.SaveAs(ms);
      }

      return ms.ToArray();
    }

    private void CreateProjectsPartTable(
      IXLWorksheet ws,
      int mainProjectsStartColumn,
      int otherProjectsStartColumn,
      List<ProjectData> mainProjects,
      List<ProjectData> otherProjects,
      List<UserData> sortedUsers,
      List<DbWorkTime> workTimes)
    {
      int mainProjectsColumn = mainProjectsStartColumn;
      int otherProjectsColumn = otherProjectsStartColumn;

      foreach (var project in mainProjects)
      {
        AddHeaderCell(ws, mainProjectsColumn, project.Name, MainProjectColor);
        mainProjectsColumn++;
      }

      foreach (var project in otherProjects)
      {
        AddHeaderCell(ws, otherProjectsColumn, project.Name, OtherProjectColor);
        otherProjectsColumn++;
      }

      for (int userNumber = 0; userNumber < sortedUsers.Count; userNumber++)
      {
        int row = userNumber + 3;
        mainProjectsColumn = mainProjectsStartColumn;
        otherProjectsColumn = otherProjectsStartColumn;

        foreach (var project in mainProjects)
        {
          WriteProjectInfo(row, mainProjectsColumn++, workTimes.FirstOrDefault(wt => wt.UserId == sortedUsers[userNumber].Id && wt.ProjectId == project.Id), ws);
        }

        foreach (var project in otherProjects)
        {
          WriteProjectInfo(row, otherProjectsColumn++, workTimes.FirstOrDefault(wt => wt.UserId == sortedUsers[userNumber].Id && wt.ProjectId == project.Id), ws);
        }
      }
    }

    private void WriteProjectInfo(int row, int column, DbWorkTime wt, IXLWorksheet ws)
    {
      if (wt is not null && (wt.ManagerWorkTime?.Hours is not null || wt.Hours.HasValue))
      {
        ws.Cell(row, column).SetValue(wt.ManagerWorkTime?.Hours ?? wt.Hours);
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
      IImportStatFilterValidator validator,
      IUserImportStatInfoMapper mapper)
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
      _mapper = mapper;
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
      List<ProjectData> otherProjects = new();
      List<ProjectUserData> projectsUsers;
      DepartmentData department = null;

      if (filter.DepartmentId.HasValue)
      {
        usersIds = (await _departmentService.GetDepartmentUsersAsync(
          filter.DepartmentId.Value,
          errors,
          byEntryDate: new DateTime(filter.Year, filter.Month, 1))).usersIds;

        //todo - unite into one request
        projectsUsers = (await _projectService.GetProjectUsersAsync(errors, usersIds: usersIds)).projectUsersData;

        projects = await _projectService.GetProjectsDataAsync(
          errors,
          projectsIds: projectsUsers?.Select(pu => pu.ProjectId).Distinct().ToList(),
          includeUsers: false);

        department = (await _departmentService.GetDepartmentsDataAsync(errors, projectsIds: projects.Select(p => p.Id).ToList()))?
          .FirstOrDefault(d => d.Id == filter.DepartmentId.Value);

        if (projectsUsers is null || projects is null || department is null)
        {
          return _responseCreator.CreateFailureResponse<byte[]>(HttpStatusCode.BadRequest);
        }

        otherProjects.AddRange(projects.Where(p => !department.ProjectsIds.Contains(p.Id)).OrderBy(p => p.Name));
        projects = projects.Where(p => department.ProjectsIds.Contains(p.Id)).OrderBy(p => p.Name).ToList();
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

      if (usersInfos is null || projects is null)
      {
        return new()
        {
          Errors = errors
        };
      }

      List<CompanyUserData> companies = (await _companyService.GetCompaniesDataAsync(usersIds, errors))?.SelectMany(p => p.Users).ToList();
      List<DbWorkTime> workTimes = await _workTimeRepository.GetAsync(
        usersIds,
        projects.Select(p => p.Id).Concat(otherProjects.Select(p => p.Id).Concat(new[] {Guid.Empty})).ToList(),
        filter.Year,
        filter.Month);

      if (workTimes.Any(wt => wt.ProjectId == Guid.Empty))
      {
        otherProjects.Add(new ProjectData(id: Guid.Empty, name: "Другое", status: "Active", default, default, default));
      }

      List<DbLeaveTime> leaveTimes = await _leaveTimeRepository.GetAsync(usersIds, filter.Year, filter.Month);

      List<UserImportStatInfo> sortedUsers = _mapper.Map(usersInfos, companies)
          .GroupBy(x => x.CompanyUserData?.ContractSubject?.Name).OrderByDescending(x => x.Key).SelectMany(x => x).ToList();

      List<DbWorkTimeMonthLimit> monthsLimits;

      if (leaveTimes.Any())
      {
        monthsLimits = await GetNeededRangeOfMonthLimitsAsync(
          leaveTimes.Select(lt => lt.StartTime).Min(),
          leaveTimes.Select(lt => lt.EndTime).Max());

        if (monthsLimits is null)
        {
          errors.Add("Cannot get month limit.");

          return _responseCreator.CreateFailureResponse<byte[]>(HttpStatusCode.BadRequest, errors);
        }
      }
      else
      {
        DbWorkTimeMonthLimit monthLimit = await _workTimeMonthLimitRepository.GetAsync(filter.Year, filter.Month);

        if (monthLimit is null)
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
        Body = CreateTable(
          filter: filter,
          sortedUsers: sortedUsers,
          mainProjects: projects,
          otherProjects: otherProjects,
          department: department,
          workTimes: workTimes,
          leaveTimes: leaveTimes,
          monthsLimits: monthsLimits.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList())
      };
    }
  }
}
