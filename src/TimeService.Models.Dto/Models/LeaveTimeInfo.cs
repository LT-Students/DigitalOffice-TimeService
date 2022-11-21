using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public record LeaveTimeInfo
  {
    public Guid Id { get; set; }
    public int Minutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public LeaveType LeaveType { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Comment { get; set; }
    public bool IsClosed { get; set; }
    public bool IsActive { get; set; }

    public ManagerLeaveTimeInfo ManagerLeaveTime { get; set; }
    public UserInfo ManagerInfo { get; set; }
  }
}
