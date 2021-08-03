﻿using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
    public record LeaveTimeInfo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CreatedBy { get; set; }
        public int Minutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Comment { get; set; }
        public bool IsActive { get; set; }
    }
}
