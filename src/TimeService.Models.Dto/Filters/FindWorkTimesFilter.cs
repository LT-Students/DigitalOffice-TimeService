using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
    public class FindWorkTimesFilter
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
    }
}
