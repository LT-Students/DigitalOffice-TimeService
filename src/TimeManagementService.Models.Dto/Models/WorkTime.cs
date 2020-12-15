using System;
using System.Text.Json.Serialization;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto.Models
{
    public class WorkTime
    {
        public Guid? Id { get; set; }
        public Guid UserId { get; set; }
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
