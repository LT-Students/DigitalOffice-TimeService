using System;
using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public record FindStatFilter : BaseFindFilter
  {
    [FromQuery(Name = "departmentid")]
    public List<Guid> DepartmentsIds { get; set; }

    [FromQuery(Name = "projectid")]
    public List<Guid> ProjectsIds { get; set; }

    [FromQuery(Name = "year")]
    public int Year { get; set; }

    [FromQuery(Name = "month")]
    public int Month { get; set; }

    [FromQuery(Name = "ascendingsort")]
    public bool? AscendingSort { get; set; }

    [FromQuery(Name = "nameincludesubstring")]
    public string NameIncludeSubstring { get; set; }
  }
}
