using System;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public class FindLeaveTimesFilter
  {
    [FromQuery(Name = "userid")]
    public Guid? UserId { get; set; }

    [FromQuery(Name = "starttime")]
    public DateTime? StartTime { get; set; }

    [FromQuery(Name = "endtime")]
    public DateTime? EndTime { get; set; }

    [FromQuery(Name = "skipCount")]
    public int SkipCount { get; set; }

    [FromQuery(Name = "takeCount")]
    public int TakeCount { get; set; }

    [FromQuery(Name = "includedeactivated")]
    public bool? IncludeDeactivated { get; set; }
  }
}
