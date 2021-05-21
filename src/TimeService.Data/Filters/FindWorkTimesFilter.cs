using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeService.Data.Filters
{
    public class FindWorkTimesFilter
    {
        [FromQuery(Name = "userid")]
        public Guid? UserId { get; set; }

        [FromQuery(Name = "starttime")]
        public DateTime? StartTime { get; set; }

        [FromQuery(Name = "endtime")]
        public DateTime? EndTime { get; set; }
    }
}
