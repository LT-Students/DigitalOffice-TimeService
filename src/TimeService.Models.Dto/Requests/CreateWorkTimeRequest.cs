using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Requests
{
  public record CreateWorkTimeRequest
  {
    public Guid UserId { get; set; }
    public float? Hours { get; set; }
    public string Description { get; set; }
  }
}
