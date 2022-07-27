using System;
using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public record FindStatFilter : BaseFindFilter
  {
    [FromQuery(Name = "departmentsids")]
    public List<Guid> DepartmentsIds { get; set; }

    [FromQuery(Name = "projectid")]
    public Guid? ProjectId { get; set; }

    [FromQuery(Name = "year")]
    public int Year { get; set; }

    [FromQuery(Name = "month")]
    public int? Month { get; set; }

    [FromQuery(Name = "acsendingsort")]
    public bool? AscendingSort { get; set; }

    [FromQuery(Name = "nameincludesubstring")]
    public string NameIncludeSubstring { get; set; }
  }
}
