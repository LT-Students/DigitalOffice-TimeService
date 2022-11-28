using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Requests
{
  public record EditLeaveTimeRequest
  {
    public int Minutes { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string Comment { get; set; }
    public bool IsClosed { get; set; }
    public bool IsActive { get; set; }
  }
}
