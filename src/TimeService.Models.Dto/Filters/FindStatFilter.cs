using System;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public record FindStatFilter
  {
    [FromQuery(Name = "departmentid")]
    public Guid? DepartmentId { get; set; }

    [FromQuery(Name = "projectid")]
    public Guid? ProjectId { get; set; }

    [FromQuery(Name = "skipcount")]
    public int SkipCount { get; set; }

    [FromQuery(Name = "takecount")]
    public int TakeCount { get; set; }

    [FromQuery(Name = "year")]
    public int Year { get; set; }

    [FromQuery(Name = "month")]
    public int Month { get; set; }
  }
}
