using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using System;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto.Models
{
    public class LeaveTime
    {
        public Guid? Id { get; set; }
        public Guid WorkerUserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Comment { get; set; }
    }
}
