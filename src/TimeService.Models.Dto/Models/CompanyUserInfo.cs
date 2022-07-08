using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public record CompanyUserInfo
  {
    public double? Rate { get; set; }
    public DateTime StartWorkingAt { get; set; }
  }
}
