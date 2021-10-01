using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
    public class FindWorkTimeMonthLimitsFilter
    {
        [FromQuery(Name = "year")]
        public int? Year { get; set; }

        [FromQuery(Name = "month")]
        public int? Month { get; set; }
    }
}
