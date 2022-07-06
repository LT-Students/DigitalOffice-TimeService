using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public record UserStatInfo
  {
    public UserInfo User { get; set; }

    public PositionInfo Position { get; set; }

    public CompanyUserInfo CompanyUserInfo { get; set; }

    public List<LeaveTimeInfo> LeaveTimes { get; set; }

    public List<WorkTimeInfo> WorkTimes { get; set; }

    public WorkTimeMonthLimitInfo LimitInfo { get; set; }
  }
}
