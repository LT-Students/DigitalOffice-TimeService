using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
    public record WorkTimeInfo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ProjectInfo Project { get; set; }
        public Guid WorkTimeMonthLimitId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public float UserHours { get; set; }
        public float ManagerHours { get; set; }
        public string Description { get; set; }
    }
}
