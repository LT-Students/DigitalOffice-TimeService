using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Requests
{
    public record EditWorkTimeRequest
    {
        public int Day { get; set; }
        public float? UserHours { get; set; }
        public float? ManagerHours { get; set; }
        public string Description { get; set; }
    }
}
