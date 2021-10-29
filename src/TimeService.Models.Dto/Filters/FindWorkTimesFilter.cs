using System;
using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public record FindWorkTimesFilter : BaseFindFilter
  {
    [FromQuery(Name = "userid")]
    public Guid? UserId { get; set; }

    [FromQuery(Name = "projectid")]
    public Guid? ProjectId { get; set; }

    [FromQuery(Name = "month")]
    public int? Month { get; set; }

    [FromQuery(Name = "year")]
    public int? Year { get; set; }

    [FromQuery(Name = "includedeactivated")]
    public bool? IncludeDeactivated { get; set; }

    [FromQuery(Name = "includedayjobs")]
    public bool? IncludeDayJobs { get; set; }
  }
}
