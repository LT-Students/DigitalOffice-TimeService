using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using System;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto
{
    public class CreateLeaveTimeRequest
    {
        public Guid WorkerUserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Comment { get; set; }
    }
}
