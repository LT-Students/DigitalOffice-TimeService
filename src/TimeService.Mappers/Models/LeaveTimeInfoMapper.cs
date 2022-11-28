using System;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class LeaveTimeInfoMapper : ILeaveTimeInfoMapper
  {
    private readonly IManagerLeaveTimeInfoMapper _managerLeaveTimeInfoMapper;

    public LeaveTimeInfoMapper(
      IManagerLeaveTimeInfoMapper managerLeaveTimeInfoMapper)
    {
      _managerLeaveTimeInfoMapper = managerLeaveTimeInfoMapper;
    }

    public LeaveTimeInfo Map(DbLeaveTime dbLeaveTime, UserInfo managerInfo)
    {
      if (dbLeaveTime is null)
      {
        return null;
      }

      return new LeaveTimeInfo
      {
        Id = dbLeaveTime.Id,
        CreatedBy = dbLeaveTime.CreatedBy,
        Minutes = dbLeaveTime.Minutes,
        StartTime = dbLeaveTime.StartTime,
        EndTime = dbLeaveTime.EndTime,
        CreatedAtUtc = dbLeaveTime.CreatedAtUtc,
        Comment = dbLeaveTime.Comment,
        LeaveType = (LeaveType)dbLeaveTime.LeaveType,
        IsClosed = dbLeaveTime.IsClosed,
        IsActive = dbLeaveTime.IsActive,
        ManagerLeaveTime = _managerLeaveTimeInfoMapper.Map(dbLeaveTime.ManagerLeaveTime),
        ManagerInfo = managerInfo
      };
    }
  }
}
