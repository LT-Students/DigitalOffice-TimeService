using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class ManagerLeaveTimeInfoMapper : IManagerLeaveTimeInfoMapper
  {
    public ManagerLeaveTimeInfo Map(DbLeaveTime managerLeaveTime)
    {
      if (managerLeaveTime is null)
      {
        return null;
      }

      return new()
      {
        Minutes = managerLeaveTime.Minutes,
        StartTime = managerLeaveTime.StartTime,
        EndTime = managerLeaveTime.EndTime,
        LeaveType = (LeaveType)managerLeaveTime.LeaveType,
        Comment = managerLeaveTime.Comment,
        CreatedAtUtc = managerLeaveTime.CreatedAtUtc,
        IsClosed = managerLeaveTime.IsClosed,
        IsActive = managerLeaveTime.IsActive
      };
    }
  }
}
