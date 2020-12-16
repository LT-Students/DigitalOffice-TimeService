using System;
using System.Text.Json.Serialization;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto.Requests
{
    public class WorkTimeRequest
    {
        public Guid? UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Minutes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid ProjectId { get; set; }

        [JsonIgnore]
        public Guid CurrentUserId { get; set; }
    }
}
