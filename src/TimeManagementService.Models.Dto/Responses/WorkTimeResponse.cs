using System;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto.Responses
{
    public class WorkTimeResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Minutes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
