using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Models.Dto.Responses
{
  public record WorkTimeResponse
  {
    public WorkTimeInfo WorkTime { get; set; }
    public UserInfo User { get; set; }
    public UserInfo Manager { get; set; }
    public WorkTimeMonthLimitInfo LimitInfo { get; set; }
  }
}
