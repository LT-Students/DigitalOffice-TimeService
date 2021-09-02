using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ClosedXML.Excel;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Business.Commands.Import.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
    private readonly IRequestClient<IFindDepartmentUsersRequest> _rcFindDepartmentUsers;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly IUserInfoMapper _userInfoMapper;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly ILogger<ImportStatCommand> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    #region private methods

    private List<ProjectData> GetProjects(ImportStatFilter filter, List<string> errors)
    {
      string messageError = "Cannot get projects info. Please, try again later.";
      const string logError = "Cannot get projects info.";

      if (!filter.ProjectId.HasValue && !filter.DepartmentId.HasValue)
      {
        return null;
      }

      try
      {
        List<Guid> projectIds = filter.ProjectId.HasValue ? new() { filter.ProjectId.Value } : null;

        IOperationResult<IGetProjectsResponse> result = _rcGetProjects.GetResponse<IOperationResult<IGetProjectsResponse>>(
            IGetProjectsRequest.CreateObj(
              projectsIds: projectIds,
              departmentId: filter.DepartmentId,
              includeUsers: filter.ProjectId.HasValue)).Result.Message;

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

    private List<Guid> FindDepartmentUsers(Guid departmentId, List<string> errors)
    {
      string messageError = "Cannot get department users info. Please, try again later.";
      const string logError = "Cannot get users of department with id: '{id}'.";

      List<Guid> response = new();

      try
      {
        IOperationResult<IFindDepartmentUsersResponse> result = _rcFindDepartmentUsers.GetResponse<IOperationResult<IFindDepartmentUsersResponse>>(
            IFindDepartmentUsersRequest.CreateObj(departmentId, null, null)).Result.Message;

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

    private List<UserInfo> GetUsers(List<Guid> userIds, List<string> errors)
    {
      if (userIds == null || !userIds.Any())
      {
        return new();
      }

      string message = "Cannot get users data. Please try again later.";
      string loggerMessage = $"Cannot get users data for specific user ids:'{string.Join(",", userIds)}'.";

      try
      {
        var response = _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
            IGetUsersDataRequest.CreateObj(userIds)).Result;

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.UsersData.Select(_userInfoMapper.Map).ToList();
        }

        _logger.LogWarning(loggerMessage + "Reasons: {Errors}", string.Join("\n", response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, loggerMessage);
      }

      errors.Add(message);

      return new();
    }

    #region Create table

    //create this method for counting ours
    private float GetLeaveTimeOurs(DbWorkTimeMonthLimit monthLimit, DbLeaveTime leaveTime, int year, int month)
    {
      if (leaveTime.StartTime.Month == month && leaveTime.StartTime.Year == year
        || leaveTime.EndTime.Month == month && leaveTime.EndTime.Year == year)
      {
        return (float)leaveTime.Minutes / 60;
      }

      return 0;
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

    private byte[] CreateTableForProject(
      List<UserInfo> usersInfos,
      ProjectData project,
      List<DbWorkTime> workTimes,
      List<DbLeaveTime> leaveTimes,
      DbWorkTimeMonthLimit monthLimit)
    {
      //Header
      List<string> header = new()
      {
        "№",
        "Initials",
        "Rate",
        "Standard working hours",
        "Total"
      };

      MemoryStream ms = new MemoryStream();
      using (var workbook = new XLWorkbook())
      {
        IXLWorksheet ws = workbook.Worksheets.Add("Ours");

        IXLRange range = ws.Range(1, 1, 1, header.Count);

        int columnNumber = 1;
        // line 1
        foreach (var columnName in header)
        {
          AddHeaderCell(ws, columnNumber, columnName, FirstHeaderColor);
          columnNumber++;
        }

        AddHeaderCell(ws, columnNumber, project.Name, MainProjectColor);
        columnNumber++;

        foreach (var leaveName in Enum.GetValues(typeof(LeaveType)))
        {
          AddHeaderCell(ws, columnNumber, leaveName.ToString(), LeaveTypesColor);
          columnNumber++;
        }

        ws.Cell(2, 2).Value = "Total";
        ws.Cell(2, 2).Style.Font.SetBold();
        ws.Range(2, 1, 2, columnNumber - 1).Style.Fill.SetBackgroundColor(SecondHeaderColor);

        // line 2
        for (int column = 5; column < columnNumber; column++)
        {
          ws.Cell(2, column).SetFormulaR1C1($"=SUM({ws.Cell(3, column).Address}:{ws.Cell(2 + usersInfos.Count(), column).Address})");
        }

        // users lines
        for (int number = 0; number < usersInfos.Count; number++)
        {
          int row = number + 3;

          ws.Cell(row, 1).SetValue(number + 1);
          ws.Cell(row, 2).SetValue($"{usersInfos[number].FirstName} {usersInfos[number].LastName}");
          ws.Cell(row, 3).SetValue(usersInfos[number].Rate);
          ws.Cell(row, 4).SetValue(monthLimit.NormHours);
          ws.Cell(row, 5).SetFormulaR1C1($"=SUM({ws.Cell(row, 6).Address}:{ws.Cell(row, columnNumber - 1).Address})");
          ws.Cell(row, 5).Style.Fill.SetBackgroundColor(FirstHeaderColor);

          var wt = workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[number].Id);

          if (wt != null && (wt.ManagerHours.HasValue || wt.UserHours.HasValue))
          {
            ws.Cell(row, 6).SetValue(wt.ManagerHours.HasValue ? wt.ManagerHours : wt.UserHours);
          }

          int column = 7;

          foreach (var leaveTime in leaveTimes.Where(lt => lt.UserId == usersInfos[number].Id))
          {
            //TODO use method
            ws.Cell(row, column + leaveTime.LeaveType).SetValue((float)leaveTime.Minutes / 60);
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

    private byte[] CreateTableForDepartment(
      Guid departmentId,
      List<UserInfo> usersInfos,
      List<ProjectData> projects,
      List<DbWorkTime> workTimes,
      List<DbLeaveTime> leaveTimes,
      DbWorkTimeMonthLimit monthLimit)
    {
      //Header
      List<string> header = new()
      {
        "№",
        "Initials",
        "Rate",
        "Standard working hours",
        "Total"
      };

      MemoryStream ms = new MemoryStream();
      using (var workbook = new XLWorkbook())
      {
        IXLWorksheet ws = workbook.Worksheets.Add("Ours");

        IXLRange range = ws.Range(1, 1, 1, header.Count);

        int columnNumber = 1;
        // line 1
        foreach(var columnName in header)
        {
          AddHeaderCell(ws, columnNumber, columnName, FirstHeaderColor);
          columnNumber++;
        }

        List<ProjectData> departmentProjects = projects.Where(p => p.DepartmentId == departmentId).ToList();
        List<ProjectData> otherProjects = projects.Where(p => p.DepartmentId != departmentId).ToList();

        foreach (var project in departmentProjects)
        {
          AddHeaderCell(ws, columnNumber, project.Name, MainProjectColor);
          columnNumber++;
        }

        foreach (var project in otherProjects)
        {
          AddHeaderCell(ws, columnNumber, project.Name, OtherProjectColor);
          columnNumber++;
        }

        foreach(var leaveName in Enum.GetValues(typeof(LeaveType)))
        {
          AddHeaderCell(ws, columnNumber, leaveName.ToString(), LeaveTypesColor);
          columnNumber++;
        }

        ws.Cell(2, 2).Value = "Total";
        ws.Cell(2, 2).Style.Font.SetBold();
        ws.Range(2, 1, 2, columnNumber - 1).Style.Fill.SetBackgroundColor(SecondHeaderColor);

        // line 2
        for(int column = 5; column < columnNumber; column++)
        {
          ws.Cell(2, column).SetFormulaR1C1($"=SUM({ws.Cell(3, column).Address}:{ws.Cell(2 + usersInfos.Count(), column).Address})");
        }

        // users lines
        for(int number = 0; number < usersInfos.Count; number++)
        {
          int row = number + 3;

          ws.Cell(row, 1).SetValue(number + 1);
          ws.Cell(row, 2).SetValue($"{usersInfos[number].FirstName} {usersInfos[number].LastName}");
          ws.Cell(row, 3).SetValue(usersInfos[number].Rate);
          ws.Cell(row, 4).SetValue(monthLimit.NormHours);
          ws.Cell(row, 5).SetFormulaR1C1($"=SUM({ws.Cell(row, 6).Address}:{ws.Cell(row, columnNumber - 1).Address})").
            Style.Fill.SetBackgroundColor(FirstHeaderColor);

          int column = 6;
          foreach(var project in departmentProjects)
          {
            var wt = workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[number].Id && wt.ProjectId == project.Id);

            if (wt != null && (wt.ManagerHours.HasValue || wt.UserHours.HasValue))
            {
              ws.Cell(row, column).SetValue(wt.ManagerHours.HasValue ? wt.ManagerHours : wt.UserHours);
            }

            column++;
          }

          foreach (var project in otherProjects)
          {
            var wt = workTimes.FirstOrDefault(wt => wt.UserId == usersInfos[number].Id && wt.ProjectId == project.Id);

            if (wt != null && (wt.ManagerHours.HasValue || wt.UserHours.HasValue))
            {
              ws.Cell(row, column).SetValue(wt.ManagerHours.HasValue ? wt.ManagerHours : wt.UserHours);
            }

            column++;
          }

          foreach(var leaveTime in leaveTimes.Where(lt => lt.UserId == usersInfos[number].Id))
          {
            //TODO use method
            ws.Cell(row, column + leaveTime.LeaveType).SetValue((float)leaveTime.Minutes / 60);
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
    #endregion

    #endregion

    public ImportStatCommand(
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IFindDepartmentUsersRequest> rcFindDepartmentUsers,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      IUserInfoMapper userInfoMapper,
      IWorkTimeRepository workTimeRepository,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      ILogger<ImportStatCommand> logger,
      IHttpContextAccessor httpContextAccessor)
    {
      _rcGetProjects = rcGetProjects;
      _rcFindDepartmentUsers = rcFindDepartmentUsers;
      _rcGetUsers = rcGetUsers;
      _userInfoMapper = userInfoMapper;
      _workTimeRepository = workTimeRepository;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _logger = logger;
      _httpContextAccessor = httpContextAccessor;
    }

    public OperationResultResponse<byte[]> Execute(ImportStatFilter filter)
    {
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

      List<ProjectData> projects = GetProjects(filter, errors);

      if (filter.DepartmentId.HasValue)
      {
        usersIds = FindDepartmentUsers(filter.DepartmentId.Value, errors);
      }
      else
      {
        usersIds = projects?.SelectMany(p => p.Users.Select(pu => pu.UserId)).OrderBy(id => id).Distinct().ToList();
      }

      List<UserInfo> usersInfos = GetUsers(usersIds, errors);

      if (usersInfos == null || projects == null)
      {
        return new()
        {
          Status = OperationResultStatusType.Failed,
          Errors = errors
        };
      }

      List<DbWorkTime> workTimes = _workTimeRepository.Find(usersIds, filter.Year, filter.Month);
      List<DbLeaveTime> leaveTimes = _leaveTimeRepository.Find(usersIds, filter.Year, filter.Month);
      DbWorkTimeMonthLimit monthLimit = _workTimeMonthLimitRepository.Get(filter.Year, filter.Month);

      return new()
      {
        Status = OperationResultStatusType.FullSuccess,
        Body =
          filter.DepartmentId.HasValue
          ? CreateTableForDepartment(filter.DepartmentId.Value, usersInfos, projects, workTimes, leaveTimes, monthLimit)
          : CreateTableForProject(usersInfos, projects[0], workTimes, leaveTimes, monthLimit)
      };
    }
  }
}
