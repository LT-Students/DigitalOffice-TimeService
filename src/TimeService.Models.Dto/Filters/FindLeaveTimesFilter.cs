using System;
using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public record FindLeaveTimesFilter : BaseFindFilter
  {
    [FromQuery(Name = "userid")]
    public Guid? UserId { get; set; }

    [FromQuery(Name = "starttime")]
    public DateTime? StartTime { get; set; }

    [FromQuery(Name = "endtime")]
    public DateTime? EndTime { get; set; }

    [FromQuery(Name = "includedeactivated")]
    public bool? IncludeDeactivated { get; set; }
  }
}
