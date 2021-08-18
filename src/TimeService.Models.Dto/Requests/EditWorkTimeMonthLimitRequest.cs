namespace LT.DigitalOffice.TimeService.Models.Dto.Requests
{
    public record EditWorkTimeMonthLimitRequest
    {
        public float NormHours { get; set; }
        public string Holidays { get; set; }
    }
}
