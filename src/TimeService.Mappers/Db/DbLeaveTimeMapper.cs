using System;
using System.Linq;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
  public class DbLeaveTimeMapper : IDbLeaveTimeMapper
  {
    private const int WorkingDayDurationInMinutes = 480;
    private const double DefaultRate = 1;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbLeaveTimeMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbLeaveTime Map(CreateLeaveTimeRequest request, double? rate = null, string holidays = null)
    {
      if (request == null)
      {
        return null;
      }

      int? thisMonthWorkDays = holidays?.Substring(request.StartTime.Day).Count(x => x == '0');

      return new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        ParentId = null,
        UserId = request.UserId,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
        LeaveType = (int)request.LeaveType,
        Minutes = request.LeaveType == LeaveType.Prolonged
          ? (int)(thisMonthWorkDays * WorkingDayDurationInMinutes * (decimal)(rate ?? DefaultRate))
          : request.Minutes,
        StartTime = request.StartTime.UtcDateTime,
        EndTime = request.EndTime?.UtcDateTime //creates the dateTime with last day of start time's month if it is null
          ?? new DateTime(request.StartTime.UtcDateTime.Year, request.StartTime.UtcDateTime.Month, 1).AddMonths(1).AddMilliseconds(-1),
        CreatedAtUtc = DateTime.UtcNow,
        Comment = !string.IsNullOrEmpty(request.Comment?.Trim()) ? request.Comment.Trim() : null,
        IsClosed = request.LeaveType == LeaveType.Prolonged
          ? false
          : true,
        IsActive = true
      };
    }
  }
}
