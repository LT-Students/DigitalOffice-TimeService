namespace LT.DigitalOffice.TimeService.Models.Dto.Configurations
{
  public record TimeConfig
  {
    public const string SectionName = "Time";

    public int MinutesToRestart { get; set; }
    public int CountNeededNextMonth { get; set; }
  }
}
