using System;

namespace LT.DigitalOffice.TimeManagementService.Models
{
    public class CreateWorkTimeRequest
    {
        public Guid WorkerUserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Title { get; set; }
        public Guid ProjectId { get; set; }
        public string Description { get; set; }
    }
}
