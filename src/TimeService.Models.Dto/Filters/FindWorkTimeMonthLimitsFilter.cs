using LT.DigitalOffice.Kernel.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
  public record FindWorkTimeMonthLimitsFilter : BaseFindFilter
  {
    [FromQuery(Name = "year")]
    public int? Year { get; set; }

    [FromQuery(Name = "month")]
    public int? Month { get; set; }
  }
}
