using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Constants
{
  public static class CacheLifeTime
  {
    public static TimeSpan ProjectUsersLifePeriod = new(days: 31, hours: 0, minutes: 0, seconds: 0);
  }
}
