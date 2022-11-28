using System;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public class ManagerLeaveTimeInfo
  {
    public int Minutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public LeaveType LeaveType { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsClosed { get; set; }
    public bool IsActive { get; set; }
  }
}
