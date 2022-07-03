using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public record PositionInfo
  {
    public Guid PositionId { get; set; }
    public string Name { get; set; }
  }
}
