﻿using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
    public record WorkTimeDayJobInfo
    {
        public Guid Id { get; set; }
        public Guid WorkTimeId { get; set; }
        public int Day { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Minutes { get; set; }
        public bool IsActive { get; set; }
    }
}