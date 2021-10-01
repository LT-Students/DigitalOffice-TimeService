using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Models.Dto.Responses
{
  public record LeaveTimeResponse
  {
    public LeaveTimeInfo LeaveTime { get; set; }
    public UserInfo User { get; set; }
  }
}
