using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Requests
{
    public class CreateLeaveTimeRequest
    {
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Comment { get; set; }
    }
}
