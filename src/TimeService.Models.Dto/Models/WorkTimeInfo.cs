using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
    public class WorkTimeInfo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
