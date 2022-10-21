using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LT.DigitalOffice.TimeService.Models.Dto.Enums
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum LeaveType
  {
    Vacation = 0,
    SickLeave = 1,
    Training = 2,
    Idle = 3,
    Prolonged = 4
  }
}
