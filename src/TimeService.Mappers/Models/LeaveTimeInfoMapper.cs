using System;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class LeaveTimeInfoMapper : ILeaveTimeInfoMapper
  {
    public LeaveTimeInfo Map(DbLeaveTime dbLeaveTime)
    {
      if (dbLeaveTime == null)
      {
        throw new ArgumentNullException(nameof(dbLeaveTime));
      }

      return new LeaveTimeInfo
      {
        Id = dbLeaveTime.Id,
        CreatedBy = dbLeaveTime.CreatedBy,
        Minutes = dbLeaveTime.Minutes,
        StartTime = dbLeaveTime.StartTimeUtc,
        EndTime = dbLeaveTime.EndTimeUtc,
        CreatedAt = dbLeaveTime.CreatedAtUtc,
        Comment = dbLeaveTime.Comment,
        LeaveType = (LeaveType)dbLeaveTime.LeaveType,
        IsActive = dbLeaveTime.IsActive
      };
    }
  }
}
