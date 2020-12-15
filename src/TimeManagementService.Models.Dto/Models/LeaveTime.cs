using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using System;
using System.Text.Json.Serialization;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto.Models
{
    public class LeaveTime
    {
        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Comment { get; set; }
        [JsonIgnore]
        public Guid CurrentUserId { get; set; }
    }
}
