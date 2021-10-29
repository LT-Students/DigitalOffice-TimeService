namespace LT.DigitalOffice.TimeService.Models.Dto.Requests
{
  public record EditWorkTimeRequest
  {
    public float? Hours { get; set; }
    public string Description { get; set; }
  }
}
