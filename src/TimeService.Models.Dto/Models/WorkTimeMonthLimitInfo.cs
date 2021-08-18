using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
    public record WorkTimeMonthLimitInfo
    {
        public Guid Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public float NormHours { get; set; }
        public string Holidays { get; set; }
    }
}
