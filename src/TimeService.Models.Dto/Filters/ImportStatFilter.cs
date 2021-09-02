using System;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public record ImportStatFilter
  {
    [FromQuery(Name = "departmentId")]
    public Guid? DepartmentId { get; set; }

    [FromQuery(Name = "projectId")]
    public Guid? ProjectId { get; set; }

    [FromQuery(Name = "year")]
    public int Year { get; set; }

    [FromQuery(Name = "month")]
    public int Month { get; set; }
  }
}
