using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using System;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto.Responses
{
    public class LeaveTimeResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Comment { get; set; }
    }
}
