﻿using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Filters
{
    public class FindLeaveTimesFilter
    {
        [FromQuery(Name = "userid")]
        public Guid? UserId { get; set; }

        [FromQuery(Name = "starttime")]
        public DateTime? StartTime { get; set; }

        [FromQuery(Name = "endtime")]
        public DateTime? EndTime { get; set; }
    }
}