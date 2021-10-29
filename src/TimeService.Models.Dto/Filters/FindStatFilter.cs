using System;
using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public record FindStatFilter : BaseFindFilter
  {
    [FromQuery(Name = "departmentid")]
    public Guid? DepartmentId { get; set; }

    [FromQuery(Name = "projectid")]
    public Guid? ProjectId { get; set; }

    [FromQuery(Name = "year")]
    public int Year { get; set; }

    [FromQuery(Name = "month")]
    public int Month { get; set; }
  }
}
